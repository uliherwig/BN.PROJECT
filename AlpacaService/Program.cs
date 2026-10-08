
var builder = WebApplication.CreateBuilder(args);

//ConfigureLogging(builder.Host);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

MigrateDatabase(app);
app.MapHealthChecks("/health");
app.MapHub<AlpacaHub>("/alpacahub");


app.Run();

//static void ConfigureLogging(IHostBuilder hostBuilder)
//{
//    Log.Logger = new LoggerConfiguration()
//        .MinimumLevel.Information()
//        .WriteTo.Console()
//        .WriteTo.Seq("http://localhost:9017")
//        .CreateLogger();
//    hostBuilder.UseSerilog(Log.Logger);
//}

static void ConfigureMiddleware(WebApplication app)
{
    app.UseMiddleware<GlobalExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.MapControllers();

    app.UseCors("CorsPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("AlpacaDbConnection");
    services.AddDbContext<AlpacaDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddHttpContextAccessor();  // TODO check if needed for services that require access to HttpContext

    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BN Project Alpaca API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = ""
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    services.AddKeyCloakAuthentication(configuration);

    services.AddHttpClient();
    services.AddHealthChecks();
    services.AddHttpClient<IStrategyServiceClient, StrategyServiceClient>();
    services.AddHttpClient<IFinAIServiceClient, FinAIServiceClient>();

    // Singleton: the Alpaca streaming connection and its subscriptions must persist across requests.
    services.AddSingleton<IAlpacaClient, AlpacaClient>();
    services.AddScoped<IAlpacaRepository, AlpacaRepository>();
    services.AddSingleton<IAlpacaDataService, AlpacaDataService>();
    services.AddScoped<IAlpacaTradingService, AlpacaTradingService>();
    services.AddScoped<IStrategyTestService, StrategyTestService>();
    services.AddScoped<IStartUpService, StartUpService>();
    services.AddScoped<IPositionLifecycleService, PositionLifecycleService>();
    services.AddHostedService<MessageConsumerService>();

    var redisConnection = configuration["RedisConnection"]
        ?? throw new InvalidOperationException("Missing configuration: RedisConnection");
    var redis = ConnectionMultiplexer.Connect(redisConnection);

    // Register both the interface and the concrete type so DI can resolve either.
    services.AddSingleton<IConnectionMultiplexer>(redis);


    // Register publisher/subscriber services
    services.AddScoped<IRedisPublisher, RedisPublisher>();
    services.AddScoped<IRedisSubscriber, RedisSubscriber>();
    // Singleton: only wraps the singleton IConnectionMultiplexer, and is consumed by the singleton IAlpacaDataService.
    services.AddSingleton<IRedisStreamPublisher, RedisStreamPublisher>();
    services.AddSingleton<IRedisService, RedisService>();

    services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.AbortOnConnectFail = false;
        options.Configuration.ChannelPrefix = RedisChannel.Literal("SignalR");
    });

    // Quartz-Services
    services.AddQuartz();
    services.AddQuartzHostedService(opt =>
    {
        opt.WaitForJobsToComplete = true;
    });

    services.AddHostedService<AlpacaHistoryService>();
    services.AddHostedService<PositionManagementService>();

    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
    });
}

static void MigrateDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AlpacaDbContext>();
        context.Database.Migrate();
    }
}
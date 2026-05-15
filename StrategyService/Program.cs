var builder = WebApplication.CreateBuilder(args);

// ConfigureLogging(builder.Host);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

MigrateDatabase(app);
app.MapHealthChecks("/health");


app.Run();

// static void ConfigureLogging(IHostBuilder hostBuilder)
// {
//     Log.Logger = new LoggerConfiguration()
//         .MinimumLevel.Information()
//         .WriteTo.Console()
//         .WriteTo.Seq("http://localhost:9017")
//         .CreateLogger();
//     hostBuilder.UseSerilog(Log.Logger);
// }

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
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("BNProjectDbConnection");
    services.AddDbContext<StrategyDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddHttpContextAccessor();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BN Project Stategy API", Version = "v1" });

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

    var redisConnection = configuration["RedisConnection"];
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    services.AddSingleton<IConnectionMultiplexer>(redis);


    // Register your publisher/subscriber services

    services.AddScoped<IRedisPublisher, RedisPublisher>();
    services.AddScoped<IRedisSubscriber, RedisSubscriber>();
    services.AddScoped<IRedisParquetService, RedisParquetService>();
    
    // Quartz-Services
    services.AddQuartz();
    services.AddQuartzHostedService(opt =>
    {
        opt.WaitForJobsToComplete = true;
    });

    services.AddHostedService<CleanUpService>();
    services.AddHostedService<MessageConsumerService>();

    services.AddSingleton<IStrategyServiceStore, StrategyServiceStore>();

    services.AddScoped<IStrategyRepository, StrategyRepository>();

    services.AddWithAllDerivedTypes<IStrategyService>();  // adds all classes that implement IStrategyService as Scoped
    services.AddWithAllDerivedTypes<IIndicatorService>();  // adds all classes that implement IIndicatorService as Scoped


    services.AddScoped<IFinAIServiceClient, FinAIServiceClient>();

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

    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });
    });

}

static void MigrateDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<StrategyDbContext>();
        context.Database.Migrate();
    }
}
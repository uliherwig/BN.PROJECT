var builder = WebApplication.CreateBuilder(args);

//ConfigureLogging(builder.Host);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

ConfigureEndpoints(app);

MigrateDatabase(app);
app.MapHealthChecks("/health");

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
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("AlpacaDbConnection");
    services.AddDbContext<AlpacaDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddHttpContextAccessor();

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
    services.AddMessageBus(configuration);

    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();

    services.AddHttpClient<IStrategyServiceClient, StrategyServiceClient>();

    services.AddScoped<IAlpacaClient, AlpacaClient>();
    services.AddScoped<IAlpacaRepository, AlpacaRepository>();
    services.AddScoped<IAlpacaDataService, AlpacaDataService>();
    services.AddScoped<IAlpacaTradingService, AlpacaTradingService>();
    services.AddScoped<IStrategyTestService, StrategyTestService>();

    services.AddHostedService<MessageConsumerService>();

    // Quartz-Services
    services.AddQuartz();
    services.AddQuartzHostedService(opt =>
    {
        opt.WaitForJobsToComplete = true;
    });

    // Register QuartzHostedService
    services.AddHostedService<SendQuoteTaskService>();
    services.AddHostedService<AlpacaHistoryService>();
    services.AddControllers();
    services.AddHealthChecks();
}
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
    app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    app.UseAuthentication();
    app.UseAuthorization();
}
static void ConfigureEndpoints(WebApplication app)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
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
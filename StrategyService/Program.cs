using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigureLogging(builder.Host);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

ConfigureEndpoints(app);

MigrateDatabase(app);
app.MapHealthChecks("/health");

app.Run();

static void ConfigureLogging(IHostBuilder hostBuilder)
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        //.WriteTo.Seq("http://localhost:9017")
        .CreateLogger();
    hostBuilder.UseSerilog(Log.Logger);
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

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("BNProjectDbConnection");
    services.AddDbContext<StrategyDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
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
    services.AddMessageBus(configuration);

    services.AddHttpClient();
    services.AddHealthChecks();

    services.AddScoped<IStrategyRepository, StrategyRepository>();
    services.AddSingleton<IStrategyOperations, StrategyOperations>();

    services.AddHostedService<MessageConsumerService>();

    // Quartz-Services
    services.AddQuartz();
    services.AddQuartzHostedService(opt =>
    {
        opt.WaitForJobsToComplete = true;
    });

    services.AddHostedService<BacktestCleanUpService>();

    services.AddWithAllDerivedTypes<IStrategyService>();  // adds all classes that implement IStrategyService as Singleton
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
        var context = services.GetRequiredService<StrategyDbContext>();
        context.Database.Migrate();
    }
}
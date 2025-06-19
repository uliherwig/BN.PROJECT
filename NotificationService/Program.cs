var builder = WebApplication.CreateBuilder(args);

//ConfigureLogging(builder.Host);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

ConfigureEndpoints(app);

app.MapHub<NotificationHub>("/notificationhub");
app.MapHealthChecks("/health");

app.Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{ 
    services.AddControllers();
    services.AddHttpContextAccessor();

    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BN Project Socket API", Version = "v1" });

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
   
    services.AddHostedService<MessageConsumerService>();

    var redisConnection = configuration["RedisConnection"];
    var redis = ConnectionMultiplexer.Connect(redisConnection);
    services.AddSingleton<IConnectionMultiplexer>(redis);

    services.AddSignalR()
    .AddStackExchangeRedis(redisConnection, options =>
    {
        options.Configuration.AbortOnConnectFail = false;
        options.Configuration.ChannelPrefix = "SignalR";
    });


    services.AddControllers();
    services.AddHealthChecks();
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
    app.UseCors("CorsPolicy");
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

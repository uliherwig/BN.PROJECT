var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);



var path = $"{builder.Configuration["ASPNETCORE_Kestrel:Certificates:Path"]}";
var password = $"{builder.Configuration["ASPNETCORE_Kestrel:Certificates:Password"]}";


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5044); // HTTP
    options.ListenAnyIP(7283, listenOptions =>
    {
        listenOptions.UseHttps(path, password);
    });
});


var app = builder.Build();

ConfigureMiddleware(app);

ConfigureEndpoints(app);

MigrateDatabase(app);

app.Run();

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
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("IdentityDbConnection");

    services.AddDbContext<IdentityDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddHttpContextAccessor();
    //services.AddHealthChecks();

    services.AddKeyCloakAuthentication(configuration);

    services.AddEndpointsApiExplorer();

    services.AddSwaggerGen(c =>
    {
        // Standard Swagger setup
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BN Project API", Version = "v1" });

        // Bearer token configuration
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

    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();
    services.AddHttpClient<IKeycloakServiceClient, KeycloakServiceClient>();
    services.AddHostedService<SeedDatabaseService>();

    services.AddScoped<IIdentityRepository, IdentityRepository>();
}
static void MigrateDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<IdentityDbContext>();
        context.Database.Migrate();
    }
}
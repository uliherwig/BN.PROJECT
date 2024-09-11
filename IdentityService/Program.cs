var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// Enable Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello World!");

app.MapControllers();

app.Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();
    services.AddHttpContextAccessor();

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"{configuration["Keycloak:Authority"]}";   // "http://localhost:8089/realms/bn-algo-trade";
        options.MetadataAddress = $"{configuration["Keycloak:Host"]}/realms/{configuration["Keycloak:Realm"]}/.well-known/openid-configuration";
        options.Audience = configuration["Keycloak:Realm"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ValidateIssuer = true,
            ValidIssuers = [$"{configuration["Keycloak:Authority"]}"],
            ValidateAudience = true,
            ValidAudiences = ["account"],
        };
    });
    services.AddAuthorization();

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
    services.AddHostedService<KeycloakConfigStartUp>();
    services.AddScoped<IKeycloakService, KeycloakService>();
    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();

    services.AddSingleton<IConfiguration>(configuration);
    services.AddSingleton<IAuthorizationPolicyProvider, MinimumAgePolicyProvider>();
    services.AddSingleton<IAuthorizationHandler, MinimumAgeAuthorizationHandler>();



}
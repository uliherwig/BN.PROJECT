var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

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


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AlpacaDbContext>();
    context.Database.Migrate();
}

app.Run();

static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("AlpacaDbConnection");

    services.AddDbContext<AlpacaDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddHttpContextAccessor();

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = $"{configuration["Keycloak:Authority"]}";
        options.MetadataAddress = $"{configuration["Keycloak:Host"]}/realms/{configuration["Keycloak:Realm"]}/.well-known/openid-configuration";
        options.Audience = configuration["Keycloak:Realm"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = "roles",
            ValidateIssuer = true,
            ValidIssuers = [$"{configuration["Keycloak:Authority"]}"],
            ValidateAudience = true,
            ValidAudiences = ["account"],
            AuthenticationType = "Bearer"
        };
    });
    services.AddAuthorization();

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
 

    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();

    services.AddScoped<IAlpacaRepository, AlpacaRepository>();
    services.AddScoped<IAlpacaDataService, AlpacaDataService>();
    services.AddScoped<IAlpacaTradingService, AlpacaTradingService>();

    // Quartz-Services
    services.AddQuartz();
    services.AddQuartzHostedService(opt =>
    {
        opt.WaitForJobsToComplete = true;
    });

    // Register QuartzHostedService
    services.AddHostedService<AlpacaHistoryService>();
    services.AddControllers();
}
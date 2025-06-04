namespace BN.PROJECT.Core;

public static class AuthenticationExtension
{
    public static IServiceCollection AddKeyCloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
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
                    ValidIssuers = new[] { $"{configuration["Keycloak:Authority"]}" },
                    ValidateAudience = true,
                    ValidAudiences = new[] { "account" },
                    AuthenticationType = "Bearer"
                };
            });

            services.AddAuthorization();
        }
        catch (Exception ex)
        {
            throw new Exception("Error configuring Keycloak authentication", ex);
        }

        return services;
    }
}
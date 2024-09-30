using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BNProjectDbConnection");

Console.WriteLine($"Connection String: {connectionString}");

builder.Services.AddDbContext<BNProjectDbContext>(options =>
    options.UseNpgsql(connectionString));

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
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddKeyCloakAuthentication(configuration);
    services.AddMessageBus(configuration);

    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();
    services.AddScoped<IStrategyService, StrategyService>();
    services.AddScoped<PositionManager>();
}
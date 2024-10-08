var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:9017")
    .CreateLogger();
builder.Host.UseSerilog(Log.Logger);

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

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<StrategyDbContext>();
    context.Database.Migrate();
}

app.Run();
static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("BNProjectDbConnection");
    services.AddDbContext<StrategyDbContext>(options =>
        options.UseNpgsql(connectionString));

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddKeyCloakAuthentication(configuration);
    services.AddMessageBus(configuration);

    services.AddHttpClient();
    services.AddHttpClient<KeycloakAuthorizeAttribute>();
    services.AddScoped<IStrategyRepository, StrategyRepository>(); 

    //services.AddSingleton<ITestQueueService<string, string>>(new TestQueueService<string, string>(100));
    services.AddSingleton<IStrategyService, StrategyService>();

    services.AddHostedService<QuoteConsumerService>();
}
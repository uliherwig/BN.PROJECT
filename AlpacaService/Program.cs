using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});
builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});

ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ConfigureServices(IServiceCollection services)
{
   

    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddHttpClient();
    services.AddHttpClient<DataServiceClient>();

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
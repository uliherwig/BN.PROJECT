using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading;
using System.Threading.Tasks;

public class AlpacaHistoryService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public AlpacaHistoryService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {         

            var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
            var scheduler = await schedulerFactory.GetScheduler();

            // define the job and tie it to our HelloJob class
            var job = JobBuilder.Create<AlpacaHistoryJob>()
                .WithIdentity("historyJob", "alpacaGroup")          
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("alpacaTrigger", "alpacaGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(3600)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Hier kannst du Logik zum Stoppen des Dienstes hinzufügen, falls erforderlich
        return Task.CompletedTask;
    }
}

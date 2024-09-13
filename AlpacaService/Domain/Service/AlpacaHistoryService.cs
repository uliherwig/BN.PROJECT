namespace BN.PROJECT.AlpacaService
{
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

                var job = JobBuilder.Create<AlpacaHistoryJob>()
                    .WithIdentity("historyJob", "alpacaGroup")
                    .SetJobData(new JobDataMap { { "key", "AlpacaHistoryJob" } })
                    .Build();

                var trigger = TriggerBuilder.Create()
                   .WithIdentity("alpacaTrigger", "alpacaGroup")
                   .StartNow()
                   .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
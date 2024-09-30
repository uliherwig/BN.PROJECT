namespace BN.PROJECT.Core;
public static class MessageBusExtension
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IKafkaProducerHostedService, KafkaProducerService>();
        services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();

        return services;
    }
}

using BN.PROJECT.Core;

public static class MessageBusExtension
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        // Register KafkaProducerService with configuration
        services.AddSingleton<IKafkaProducerService>(provider =>
             new KafkaProducerService(configuration));

        // Register KafkaConsumerService with configuration
        services.AddSingleton<IKafkaConsumerService>(provider =>
            new KafkaConsumerService(configuration));

        return services;
    }
}
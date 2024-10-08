namespace BN.PROJECT.Core;

public static class KafkaUtilities
{
    public static string CreateTopic(string topic, string email)
    {
        return $"{topic}_{email.ToLower().Replace('@', '-').Replace('.', '-')}";
    }
}

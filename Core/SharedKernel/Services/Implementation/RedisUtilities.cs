namespace BN.PROJECT.Core;

public static class RedisUtilities
{
    public static string CreateTopic(string topic, string email)
    {
        return $"{topic}_{email.ToLower().Replace('@', '-').Replace('.', '-')}";
    }

    public static string GetChannelName(RedisChannelEnum topic) => topic.ToString().ToLowerInvariant();

}
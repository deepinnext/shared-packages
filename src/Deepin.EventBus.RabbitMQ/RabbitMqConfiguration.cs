namespace Deepin.EventBus.RabbitMQ;
public class RabbitMqConfiguration
{
    public static string ConfigurationKey = "RabbitMQ";
    public required string Host { get; set; }
    public required ushort Port { get; set; }
    public required string VirtualHost { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string QueueName { get; set; }
}

namespace VentionTask1.WebApi.Settings
{
    public class RabbitMqOptions
    {
        public string Host { get; set; } = "localhost";
        public ushort Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";

        public string DeadLetterExchange { get; set; } = "file-processing-dlx";
        public string DeadLetterRoutingKey { get; set; } = "file-processing-dead";

        public ushort PrefetchCount { get; set; } = 8;
        public int ConcurrentMessageLimit { get; set; } = 4;
    }
}

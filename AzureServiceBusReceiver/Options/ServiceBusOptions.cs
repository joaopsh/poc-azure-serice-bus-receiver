namespace AzureServiceBusReceiver.Options
{
    public class ServiceBusOptions
    {
        public const string OptionName = "ServiceBus";

        public string ServiceBusConnectionString { get; set; }

        public string QueueName { get; set; }

        public int MaxConcurrentCalls { get; set; }

        public int PrefetchCount { get; set; }
    }
}
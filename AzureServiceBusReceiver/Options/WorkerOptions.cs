namespace AzureServiceBusReceiver.Options
{
    public class WorkerOptions
    {
        public const string OptionName = "Worker";

        public int ConsumersCount { get; set; }
    }
}
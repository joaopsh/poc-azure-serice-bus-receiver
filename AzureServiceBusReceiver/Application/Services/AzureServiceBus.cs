using AzureServiceBusReceiver.Application.Services.Interfaces;
using AzureServiceBusReceiver.Options;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusReceiver.Application.Services
{
    public class AzureServiceBus : IServiceBus
    {
        private readonly ILogger<AzureServiceBus> _logger;
        private readonly IOptions<ServiceBusOptions> _serviceBusOptions;
        private IQueueClient _queueClient;

        public AzureServiceBus(ILogger<AzureServiceBus> logger, IOptions<ServiceBusOptions> serviceBusOptions)
        {
            _logger = logger;
            _serviceBusOptions = serviceBusOptions;
        }

        public void Init()
        {
            _logger.LogInformation("Starting Azure Service Bus...");

            _queueClient = new QueueClient(_serviceBusOptions.Value.ServiceBusConnectionString, _serviceBusOptions.Value.QueueName);
            _queueClient.PrefetchCount = _serviceBusOptions.Value.PrefetchCount;
            RegisterConsumer();
        }

        private void RegisterConsumer()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = _serviceBusOptions.Value.MaxConcurrentCalls,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            _logger.LogInformation($"Received message. SequenceNumber:{message.SystemProperties.SequenceNumber}.");

            await Task.Delay(1000, token);

            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _logger.LogError("Exception context for troubleshooting:");
            _logger.LogError($"- Endpoint: {context.Endpoint}");
            _logger.LogError($"- Entity Path: {context.EntityPath}");
            _logger.LogError($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing Azure Service Bus...");
            _queueClient.CloseAsync().GetAwaiter().GetResult();
        }
    }
}
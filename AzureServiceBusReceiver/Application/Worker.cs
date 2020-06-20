using AzureServiceBusReceiver.Options;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AzureServiceBusReceiver.Application
{
    public class Worker : IHostedService
    {
        private IQueueClient _queueClient;
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusOptions _serviceBusOptions;

        public Worker(ILogger<Worker> logger, IOptions<ServiceBusOptions> serviceBusOptions)
        {
            _logger = logger;
            _serviceBusOptions = serviceBusOptions.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting consumer...");

            _queueClient = new QueueClient(_serviceBusOptions.ServiceBusConnectionString, _serviceBusOptions.QueueName);
            _queueClient.PrefetchCount = _serviceBusOptions.PrefetchCount;

            RegisterConsumer();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping consumer...");
            await _queueClient.CloseAsync();
        }

        private void RegisterConsumer()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = _serviceBusOptions.MaxConcurrentCalls,
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
    }
}

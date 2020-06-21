using AzureServiceBusReceiver.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureServiceBusReceiver.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AzureServiceBusReceiver.Application
{
    public class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<WorkerOptions> _workOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly IList<IServiceBus> _serviceBuses = new List<IServiceBus>();

        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> workOptions, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _workOptions = workOptions;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting worker...");

            for (int counter = 0; counter < _workOptions.Value.ConsumersCount; counter++)
            {
                var serviceBus = _serviceProvider.GetService<IServiceBus>();
                serviceBus.Init();

                _serviceBuses.Add(serviceBus);
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping worker...");

            foreach (var serviceBus in _serviceBuses)
            {
                serviceBus.Dispose();
            }
        }
    }
}

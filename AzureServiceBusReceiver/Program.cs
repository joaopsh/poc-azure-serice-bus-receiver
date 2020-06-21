using AzureServiceBusReceiver.Application;
using AzureServiceBusReceiver.Application.Services;
using AzureServiceBusReceiver.Application.Services.Interfaces;
using AzureServiceBusReceiver.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AzureServiceBusReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServiceBusOptions>(hostContext.Configuration.GetSection(ServiceBusOptions.OptionName));
                    services.Configure<WorkerOptions>(hostContext.Configuration.GetSection(WorkerOptions.OptionName));
                    services.AddLogging(configure => configure.AddSerilog());
                    services.AddTransient<IServiceBus, AzureServiceBus>();
                    services.AddHostedService<Worker>();

                    ConfigureSerilog(hostContext.Configuration);
                });
        }

        private static void ConfigureSerilog(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}

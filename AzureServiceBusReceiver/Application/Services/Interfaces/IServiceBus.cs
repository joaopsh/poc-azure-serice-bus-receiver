using System;

namespace AzureServiceBusReceiver.Application.Services.Interfaces
{
    public interface IServiceBus : IDisposable
    {
        void Init();
    }
}
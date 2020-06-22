using NServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.TestConsole
{
    public class TestMessageBus
    {
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }
        public DirectoryInfo StorageDirectory { get; private set; }
        public async Task Start(DirectoryInfo storageDirectory)
        {
            if(!storageDirectory.Exists)
            {
                throw new Exception("Messagebus storage directory does not exist");
            }
            StorageDirectory = storageDirectory;

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.TestMessageBus");
            endpointConfiguration
                .UseNewtonsoftJsonSerializer()
                .UseTransport<LearningTransport>()
                .StorageDirectory(storageDirectory.FullName);

            _endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            IsRunning = true;
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop();
            IsRunning = false;
        }

        public Task Publish(object message)
        {
            return _endpointInstance.Publish(message);
        }
    }
}

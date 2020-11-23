using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands.Types;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestMessageBus
    {
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }
        public DirectoryInfo StorageDirectory { get; private set; }
        public async Task Start(DirectoryInfo testDirectory)
        {
            StorageDirectory = new DirectoryInfo(Path.Combine(testDirectory.FullName, ".learningtransport"));
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
            }
            
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers");
            endpointConfiguration
                .UseNewtonsoftJsonSerializer()
                .UseMessageConventions()
                .UseTransport<LearningTransport>()
                .StorageDirectory(StorageDirectory.FullName);

            endpointConfiguration.UseLearningTransport(s => s.AddRouting());

            _endpointInstance = await Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
            
            IsRunning = true;
        }

        public async Task Stop()
        {
            await _endpointInstance.Stop().ConfigureAwait(false);
            IsRunning = false;
        }

        public Task Publish(object message)
        {
            return _endpointInstance.Publish(message);
        }

        public Task Send(object message)
        {
            return _endpointInstance.Send(message);
        }
    }
}

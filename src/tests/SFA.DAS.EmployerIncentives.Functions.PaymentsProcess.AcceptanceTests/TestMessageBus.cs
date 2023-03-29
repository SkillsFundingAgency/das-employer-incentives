using NServiceBus;
using SFA.DAS.Notifications.Messages.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class TestMessageBus
    {
        private readonly TestContext _testContext;
        private IEndpointInstance _endpointInstance;
        public bool IsRunning { get; private set; }
        public DirectoryInfo StorageDirectory { get; private set; }

        public TestMessageBus(TestContext testContext)
        {
            _testContext = testContext;
        }

        public async Task Start()
        {
            StorageDirectory = new DirectoryInfo("C:\\temp\\learningtransport");
            if (!StorageDirectory.Exists)
            {
                Directory.CreateDirectory(StorageDirectory.FullName);
            }

            var endpointConfiguration = new EndpointConfiguration(_testContext.InstanceId);
            endpointConfiguration
                .UseNewtonsoftJsonSerializer()
                .UseMessageConventions()
                .UseTransport<LearningTransport>()
                .StorageDirectory(StorageDirectory.FullName);

            const string NotificationsMessageHandler = "SFA.DAS.Notifications.MessageHandlers";
            endpointConfiguration.UseLearningTransport(s => s.RouteToEndpoint(typeof(SendEmailWithAttachmentsCommand), NotificationsMessageHandler));

            _endpointInstance = await Endpoint.Start(endpointConfiguration);
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

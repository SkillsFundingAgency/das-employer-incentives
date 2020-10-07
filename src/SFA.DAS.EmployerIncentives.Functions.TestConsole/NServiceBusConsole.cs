using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.TestConsole
{
    public class NServiceBusConsole
    {
        public async Task Run()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.TestConsole");
            var storageDirectory = Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("bin")), ".learningtransport");

            endpointConfiguration
                .UseNewtonsoftJsonSerializer()
                .UseMessageConventions()
                .UseLearningTransport(s => s.AddRouting())
                .UseTransport<LearningTransport>()
                .StorageDirectory(storageDirectory);

           /* var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers")
                  .UseMessageConventions()
                  .UseNewtonsoftJsonSerializer()
                  .UseOutbox(true)
                  .UseSqlServerPersistence(() => new SqlConnection("Data Source =.; Initial Catalog = SFA.DAS.EmployerIncentives.Database; Integrated Security = True; Pooling = False; Connect Timeout = 30"))
                  .UseUnitOfWork();

            endpointConfiguration
               .UseTransport<AzureServiceBusTransport>()
               .ConnectionString("Endpoint=sb://das-at-shared-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Xq9YUB/8z22wfmnT7Gi3081gUxbbGzaZqArdkXY16cY=");
           */
            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            string command;

            do
            {
                //var message = new AddedLegalEntityEvent
                //{
                //    AccountId = 2,
                //    AccountLegalEntityId = 2,
                //    LegalEntityId = 3,
                //    OrganisationName = "Org name"
                //};

                //await endpointInstance.Publish(message);

                var message2 = new Commands.Types.ApprenticeshipIncentive.CreateIncentiveCommand(1, Guid.NewGuid());
                await endpointInstance.Send(message2);

                Console.WriteLine("Message sent...");

                Console.WriteLine("Enter 'q' to exit..." + Environment.NewLine);
                command = Console.ReadLine();
            } while (!command.Equals("q"));


            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}

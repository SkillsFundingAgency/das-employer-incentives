using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.IO;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

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

                var message2 = new CreateIncentiveCommand(1, 2, Guid.NewGuid(), 2, "test", "test", new DateTime(2000, 1, 1), 1, new DateTime(2020, 9, 1), 0, 10001234, DateTime.Now, "joe@bloggs.com", "Course Name", new DateTime(2021, 04, 01), Phase.Phase1);
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

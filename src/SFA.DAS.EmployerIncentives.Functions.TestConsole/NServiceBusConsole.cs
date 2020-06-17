using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Events;
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
                .UseTransport<LearningTransport>()
                .StorageDirectory(storageDirectory);

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            var command = string.Empty;

            do
            {
                var message = new AddedLegalEntityEvent
                {
                    AccountId = 1
                };


                await endpointInstance.Publish(message);

                Console.WriteLine("Message sent...");

                Console.WriteLine("Enter 'q' to exit..." + Environment.NewLine);
                command = Console.ReadLine();
            } while (!command.Equals("q"));


            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}

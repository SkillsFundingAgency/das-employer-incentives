using NServiceBus;
using SFA.DAS.EmployerIncentives.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using System;
using System.Collections.Generic;
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

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);

            string command;

            do
            {
                var message2 = new Commands.Types.ApprenticeshipIncentive.CreateIncentiveCommand(1, 2, new List<CreateIncentiveCommand.IncentiveApprenticeship>());
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

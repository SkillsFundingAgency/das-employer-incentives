using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class CommandService : ICommandService
    {
        private readonly HttpClient _client;
        const string allowedTypesPrefix = "SFA.DAS.EmployerIncentives.Commands.Types.";

        public CommandService(HttpClient client)
        {
            _client = client;
        }

        public async Task Dispatch<T>(T command) where T : DomainCommand
        {
            var commandType = command.GetType();
            EnsureIsValidType(commandType);

            var commandText = JsonConvert.SerializeObject(command, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            var response = await _client.PostAsJsonAsync($"commands/{commandType.FullName.Replace(allowedTypesPrefix, "")}", commandText);
            response.EnsureSuccessStatusCode();
        }

        private void EnsureIsValidType(Type commandType)
        {
            if (!commandType.FullName.StartsWith(allowedTypesPrefix))
            {
                throw new ArgumentException($"Invalid command type {commandType.FullName}");
            }
        }
    }
}

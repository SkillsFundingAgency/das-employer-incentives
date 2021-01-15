using Microsoft.AspNetCore.Mvc.Testing;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "api")]
    public class Api
    {
        private readonly TestContext _context;

        public Api(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 5)]
        public void InitialiseApi()
        {
            var eventsHook = new Hook<object>();
            _context.Hooks.Add(eventsHook);

            var commandsHook = new Hook<ICommand>();
            _context.Hooks.Add(commandsHook);

            var webApi = new TestWebApi(_context, eventsHook, commandsHook);
            var options = new WebApplicationFactoryClientOptions
            {                
                BaseAddress = new System.Uri($"https://localhost:{GetAvailablePort(5001)}")
            };
            _context.EmployerIncentivesWebApiFactory = webApi;
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient(options));
        }

        public int GetAvailablePort(int startingPort)
        {
            if (startingPort > ushort.MaxValue) throw new ArgumentException($"Can't be greater than {ushort.MaxValue}", nameof(startingPort));
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
            var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
            var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                                                 .Concat(udpListenersEndpoints)
                                                 .Select(e => e.Port);

            return Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).FirstOrDefault();
        }
    }
}

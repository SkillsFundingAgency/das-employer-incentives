using Microsoft.AspNetCore.Mvc.Testing;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "api")]
    public class Api
    {
        private readonly AcceptanceTests.TestContext _context;

        public Api(AcceptanceTests.TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 5)]
        public void InitialiseApi()
        {
            var eventsHook = new Hook<object>();
            _context.Hooks.Add(eventsHook);

            var commandsHook = new Hook<Abstractions.Commands.ICommand>();
            _context.Hooks.Add(commandsHook);

            var webApi = new TestWebApi(_context, eventsHook, commandsHook);
            var options = new WebApplicationFactoryClientOptions
            {                
                BaseAddress = new System.Uri($"https://localhost:{GetAvailablePort(5001)}")
            };
            _context.EmployerIncentivesWebApiFactory = webApi;
            _context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient(options));
        }

        [AfterScenario(Order = 2)]
        public async Task CleanUp()
        {
            Console.WriteLine($"TESTRUN: Api CleanUp");
            var endpoint = _context.EmployerIncentivesWebApiFactory.Services.GetService(typeof(IEndpointInstance)) as IEndpointInstance;
            if(endpoint != null)
            {
                Console.WriteLine($"TESTRUN: Api endpoint stop");
                await endpoint.Stop();
            }
            _context.EmployerIncentivesWebApiFactory.Server?.Dispose();
            _context.EmployerIncentivesWebApiFactory.Dispose();
            _context.EmployerIncentiveApi?.Dispose();            
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

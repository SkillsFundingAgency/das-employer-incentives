﻿using Microsoft.AspNetCore.Mvc.Testing;
using NServiceBus;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
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
        [BeforeScenario(Order = 7)]
        public void InitialiseApi(TestContext context)
        {
            var eventsHook = new Hook<object>();
            context.Hooks.Add(eventsHook);

            var commandsHook = new Hook<Abstractions.Commands.ICommand>();
            context.Hooks.Add(commandsHook);

            var webApi = new TestWebApi(context, eventsHook, commandsHook);
            var options = new WebApplicationFactoryClientOptions
            {                
                BaseAddress = new Uri($"https://localhost:{GetAvailablePort(5001)}")
            };
            context.TestWebApi = webApi;
            context.EmployerIncentiveApi = new EmployerIncentiveApi(webApi.CreateClient(options));
        }

        [AfterScenario(Order = 8)]
        public async Task CleanUp(TestContext context)
        {
            var services = context.TestWebApi.Services;

            var endpoint = context.TestWebApi.Services.GetService(typeof(IEndpointInstance)) as IEndpointInstance;
            if (endpoint != null)
            {
                await endpoint.Stop();
            }

            context.TestWebApi.Server?.Dispose();
            context.TestWebApi.Dispose();
            context.EmployerIncentiveApi?.Dispose();            
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

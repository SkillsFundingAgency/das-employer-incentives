using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.Raw;
using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestDomainMessageHandlers : IDisposable
    {
        private readonly Dictionary<string, string> _appConfig;
        private readonly Dictionary<string, string> _hostConfig;
        private readonly TestMessageBus _testMessageBus;
        private readonly List<IHook> _messageHooks;
        private readonly TestContext _testContext;
        private IHost host;
        private List<IReceivingRawEndpoint> _receivingRawEndpoints = new List<IReceivingRawEndpoint>();
        private bool isDisposed;

        public TestDomainMessageHandlers(
            TestContext testContext)
        {
            _testContext = testContext;
            _testMessageBus = testContext.MessageBus;
            _messageHooks = testContext.Hooks;

            _hostConfig = new Dictionary<string, string>();
            _appConfig = new Dictionary<string, string>
            {
                { "EnvironmentName", "LOCAL_ACCEPTANCE_TESTS" },
                { "ConfigurationStorageConnectionString", "UseDevelopmentStorage=true" },
                //{ "ConfigNames", "SFA.DAS.EmployerIncentives" },
                { "Values:AzureWebJobsStorage", "UseDevelopmentStorage=true" },
                { "ApplicationSettings:DbConnectionString", testContext.SqlDatabase.DatabaseInfo.ConnectionString },
                { "ApplicationSettings:NServiceBusConnectionString", "UseLearningEndpoint=true" },
                { "ApplicationSettings:UseLearningEndpointStorageDirectory", Path.Combine(testContext.TestDirectory.FullName, ".learningtransport") }
            };
        }

        public async Task Start()
        {
            var startUp = new Functions.DomainMessageHandlers.Startup();

            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(a =>
                {
                    a.Sources.Clear();
                    a.AddInMemoryCollection(_hostConfig);
                })
                .ConfigureAppConfiguration(a =>
                {
                    a.Sources.Clear();
                    a.AddInMemoryCollection(_appConfig);
                    a.SetBasePath(_testContext.TestDirectory.FullName);
                })
               .ConfigureWebJobs(startUp.Configure);

            _ = hostBuilder.ConfigureServices((s) =>
            {
                s.Configure<ApplicationSettings>(a =>
                {
                    a.ApiBaseUrl = _testContext.EmployerIncentiveApi.BaseAddress.AbsoluteUri;
                    a.Identifier = "";
                });

                Commands.ServiceCollectionExtensions.AddCommandHandlers(s, AddDecorators);
                s.Replace(new ServiceDescriptor(typeof(ICommandService), new CommandService(_testContext.EmployerIncentiveApi.Client)));
                s.Decorate<ICommandService, CommandServiceWithLogging>();
                
                s.AddNServiceBus(
                    new LoggerFactory().CreateLogger<TestDomainMessageHandlers>(),
                    (o) =>
                    {
                        o.EndpointConfiguration = (endpoint) =>
                        {
                            endpoint.UseTransport<LearningTransport>().StorageDirectory(_testMessageBus.StorageDirectory.FullName);
                            Commands.Types.RoutingSettingsExtensions.AddRouting(endpoint.UseTransport<LearningTransport>().Routing());
                            return endpoint;
                        };

                        o.OnStarted = (endpoint) =>
                        {
                            _receivingRawEndpoints.Add(endpoint);
                        };

                        var hook = _messageHooks.SingleOrDefault(h => h is Hook<MessageContext>) as Hook<MessageContext>;
                        if (hook != null)
                        {
                            o.OnMessageReceived = (message) =>
                            {
                                if (hook?.OnReceived != null)
                                {
                                    hook?.OnReceived(message);
                                }
                            };
                            o.OnMessageProcessed = (message) =>
                            {
                                if (hook?.OnProcessed != null)
                                {
                                    hook?.OnProcessed(message);
                                }
                            };
                            o.OnMessageErrored = (exception, message) =>
                            {
                                if (hook?.OnErrored != null)
                                {
                                    hook?.OnErrored(exception, message);
                                }
                            };
                        }
                    });
            });

            hostBuilder.UseEnvironment("LOCAL");

            host = await hostBuilder.StartAsync(_testContext.CancellationToken);
        }

        public async Task Stop()
        {
            await host.StopAsync(_testContext.CancellationToken);

            var stopEndpointTasks = new List<Task>();

            foreach (var endpoint in _receivingRawEndpoints)
            {
                stopEndpointTasks.Add(StopEndpoint(endpoint));
            }
            await Task.WhenAll(stopEndpointTasks);
        }

        private async Task StopEndpoint(IReceivingRawEndpoint endpoint)
        {
            await endpoint.Stop().ConfigureAwait(false);
        }

        public IServiceCollection AddDecorators(IServiceCollection serviceCollection)
        {
            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerReceived<>));

            Commands.ServiceCollectionExtensions.AddCommandHandlerDecorators(serviceCollection);

            serviceCollection
                .Decorate(typeof(ICommandHandler<>), typeof(TestCommandHandlerProcessed<>));

            return serviceCollection;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing && host != null)
            {
                host.Dispose();
            }

            isDisposed = true;
        }
    }
}

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
using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICommand = SFA.DAS.EmployerIncentives.Abstractions.Commands.ICommand;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestDomainMessageHandlers : IDisposable
    {
        private readonly Dictionary<string, string> _appConfig;
        private readonly Dictionary<string, string> _hostConfig;
        private readonly TestMessageBus _testMessageBus;
        private readonly List<IHook> _messageHooks;
        private readonly TestContext _testContext;
        private readonly List<IncentivePaymentProfile> _paymentProfiles;
        private IHost host;
        private List<IReceivingRawEndpoint> _receivingRawEndpoints = new List<IReceivingRawEndpoint>();
        private bool isDisposed;
        private readonly IHook<ICommand> _commandMessageHook;
        private readonly IHook<object> _eventMessageHook;


        public TestDomainMessageHandlers(
            TestContext testContext)
        {
            _testContext = testContext;
            _testMessageBus = testContext.MessageBus;
            _messageHooks = testContext.Hooks;

            _eventMessageHook = testContext.Hooks.SingleOrDefault(h => h is Hook<object>) as IHook<object>;
            if (_eventMessageHook == null)
            {
                testContext.Hooks.Add(_eventMessageHook);
            }

            _commandMessageHook = testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as IHook<ICommand>;
            if (_commandMessageHook == null)
            {
                _commandMessageHook = new Hook<ICommand>();
                testContext.Hooks.Add(_commandMessageHook);
            }

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

            _paymentProfiles = new List<IncentivePaymentProfile>
            {
                new IncentivePaymentProfile
                {
                    IncentiveType = Enums.IncentiveType.TwentyFiveOrOverIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                    {
                        new PaymentProfile{ AmountPayable = 100, DaysAfterApprenticeshipStart = 10},
                        new PaymentProfile{ AmountPayable = 200, DaysAfterApprenticeshipStart = 20},
                    }
                },
                new IncentivePaymentProfile
                {
                    IncentiveType = Enums.IncentiveType.UnderTwentyFiveIncentive,
                    PaymentProfiles = new List<PaymentProfile>
                    {
                        new PaymentProfile{ AmountPayable = 300, DaysAfterApprenticeshipStart = 30},
                        new PaymentProfile{ AmountPayable = 400, DaysAfterApprenticeshipStart = 40},
                    }
                }
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
                    a.DbConnectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
                    a.DistributedLockStorage = "UseDevelopmentStorage=true";
                    a.AllowedHashstringCharacters = "46789BCDFGHJKLMNPRSTVWXY";
                    a.Hashstring = "Test Hashstring";
                    a.NServiceBusConnectionString = "UseLearningEndpoint=true";
                    a.IncentivePaymentProfiles = _paymentProfiles;
                });                

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

                Commands.ServiceCollectionExtensions.AddCommandHandlers(s, AddDecorators);

                s.AddTransient<IDistributedLockProvider, NullLockProvider>();
                s.Decorate<IEventPublisher>((handler, sp) => new TestEventPublisher(handler, _eventMessageHook));
                s.Decorate<ICommandPublisher>((handler, sp) => new TestCommandPublisher(handler, _commandMessageHook));
                s.AddSingleton(_commandMessageHook);
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

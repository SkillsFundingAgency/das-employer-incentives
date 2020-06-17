using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Extensibility;
using NServiceBus.Raw;
using NServiceBus.Transport;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmployerIncentives.Application.Commands;
using SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity;
using SFA.DAS.EmployerIncentives.Infrastructure;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;
using SFA.DAS.NServiceBus.AzureFunction.Configuration;
using SFA.DAS.NServiceBus.AzureFunction.Hosting;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmployerIncentives.Functions.LegalEntities.Startup))]
namespace SFA.DAS.EmployerIncentives.Functions.LegalEntities
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddNLog();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureTableStorage(options =>
                {   
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["Environment"];
                    options.PreFixConfigurationKeys = false;
                })
                .Build();

            builder.AddExecutionContextBinding();

            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            builder.Services.AddSingleton<IExtensionConfigProvider, TestNServiceBusExtensionConfigProvider>((c) =>
            {
                return new TestNServiceBusExtensionConfigProvider(
                    new NServiceBusOptions
                    {
                        EndpointConfiguration = (endpoint) =>
                        {
                            endpoint.UseTransport<LearningTransport>().StorageDirectory(@"C:\repos\SFA\das-employer-incentives\src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport");
                            //endpoint.UseTransport<LearningTransport>().StorageDirectory(@"C:\repos\SFA\das-employer-incentives\.learningtransport");
                            return endpoint;
                        },
                        OnMessageReceived = (context) =>
                        {
                            context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                            context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                            context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                            context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                            logger.LogInformation($"Received NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");

                        },
                        OnMessageErrored = (ex, context) =>
                        {
                            context.Headers.TryGetValue("NServiceBus.EnclosedMessageTypes", out string messageType);
                            context.Headers.TryGetValue("NServiceBus.MessageId", out string messageId);
                            context.Headers.TryGetValue("NServiceBus.CorrelationId", out string correlationId);
                            context.Headers.TryGetValue("NServiceBus.OriginatingEndpoint", out string originatingEndpoint);
                            logger.LogError(ex, $"Error handling NServiceBusTriggerData Message of type '{(messageType != null ? messageType.Split(',')[0] : string.Empty)}' with messageId '{messageId}' and correlationId '{correlationId}' from endpoint '{originatingEndpoint}'");
                        }
                    });
            });

            builder.Services.AddTransient<ICommandHandler<AddLegalEntityCommand>, AddLegalEntityCommandHandler>();
            builder.Services.Decorate<ICommandHandler<AddLegalEntityCommand>, CommandHandlerWithLogging<AddLegalEntityCommand>>();
        }
    }

    public class NServiceBusOptions
    {
        public Func<RawEndpointConfiguration, RawEndpointConfiguration> EndpointConfiguration { get; set; }
        public Action<MessageContext> OnMessageReceived { get; set; }
        public Action<Exception, MessageContext> OnMessageErrored { get; set; }
    }

    [Extension("NServiceBus")]
    public class TestNServiceBusExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly NServiceBusOptions _nServiceBusOptions;

        public TestNServiceBusExtensionConfigProvider(NServiceBusOptions nServiceBusOptions = null)
        {
            _nServiceBusOptions = nServiceBusOptions ?? new NServiceBusOptions();
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<NServiceBusTriggerAttribute>()
                .BindToTrigger(new TestNServiceBusTriggerBindingProvider(_nServiceBusOptions));
        }
    }

    public class TestNServiceBusTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly NServiceBusOptions _nServiceBusOptions;

        public TestNServiceBusTriggerBindingProvider(NServiceBusOptions nServiceBusOptions)
        {
            _nServiceBusOptions = nServiceBusOptions ?? new NServiceBusOptions();
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var parameter = context.Parameter;
            var attribute = parameter.GetCustomAttribute<NServiceBusTriggerAttribute>(false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            if (string.IsNullOrEmpty(attribute.Connection))
            {
                attribute.Connection = SFA.DAS.NServiceBus.AzureFunction.Configuration.EnvironmentVariables.NServiceBusConnectionString;
            }

            return Task.FromResult<ITriggerBinding>(new NServiceBusTriggerBinding(parameter, attribute, _nServiceBusOptions));
        }
    }

    public class NServiceBusTriggerBinding : ITriggerBinding
    {
        public ParameterInfo Parameter { get; }
        public NServiceBusTriggerAttribute Attribute { get; }
        private readonly NServiceBusOptions _nServiceBusOptions;

        private struct BindingNames
        {
            public const string Headers = "headers";
            public const string Dispatcher = "dispatcher";
        }

        public Type TriggerValueType => typeof(NServiceBusTriggerData);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>
        {
            {BindingNames.Headers, typeof(Dictionary<string, string>) },
            {BindingNames.Dispatcher, typeof(IDispatchMessages) }
        };

        public NServiceBusTriggerBinding(ParameterInfo parameter, NServiceBusTriggerAttribute attribute, NServiceBusOptions nServiceBusOptions)
        {
            Parameter = parameter;
            Attribute = attribute;
            _nServiceBusOptions = nServiceBusOptions ?? new NServiceBusOptions();
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!(value is NServiceBusTriggerData triggerData))
            {
                throw new ArgumentException($"Value must be of type {nameof(NServiceBusTriggerData)}", nameof(value));
            }

            object argument;

            try
            {
                var messageText = Encoding.UTF8.GetString(triggerData.Data);
                //Remove all extra invalid starting characters that have come from decoding bytes
                while (messageText.Length > 2 && messageText[0] != '{')
                {
                    messageText = messageText.Remove(0, 1);
                }

                if (Parameter.ParameterType == typeof(KeyValuePair<string, string>))
                {
                    argument = new KeyValuePair<string, string>(triggerData.Headers["NServiceBus.EnclosedMessageTypes"].Split(',')[0], messageText);
                }
                else
                {
                    argument = JsonConvert.DeserializeObject(messageText, Parameter.ParameterType);
                }


            }
            catch (Exception e)
            {
                throw new ArgumentException("Trigger data has invalid payload", nameof(value), e);
            }

            var valueBinder = new NServiceBusMessageValueBinder(Parameter, argument);

            var bindingData = new Dictionary<string, object>
            {
                {BindingNames.Headers, triggerData.Headers },
                {BindingNames.Dispatcher, triggerData.Dispatcher }
            };

            return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(new TestNServiceBusListener(context.Executor, Attribute, Parameter, _nServiceBusOptions));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor
            {
                Name = Parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "NsbMessage",
                    Description = "NServiceBus trigger fired",
                    DefaultValue = "Sample"
                }
            };
        }
    }

    public class TestNServiceBusListener : IListener
    {
        private string _poisonMessageQueue;
        private const int ImmediateRetryCount = 3;

        private readonly ITriggeredFunctionExecutor _executor;
        private readonly NServiceBusTriggerAttribute _attribute;
        private readonly ParameterInfo _parameter;
        private readonly NServiceBusOptions _nServiceBusOptions;
        private IReceivingRawEndpoint _endpoint;
        private CancellationTokenSource _cancellationTokenSource;

        public TestNServiceBusListener(
            ITriggeredFunctionExecutor executor, 
            NServiceBusTriggerAttribute attribute, 
            ParameterInfo parameter,
            NServiceBusOptions nServiceBusOptions)
        {
            _executor = executor;
            _attribute = attribute;
            _parameter = parameter;
            _poisonMessageQueue = $"{attribute.Endpoint}-Error";
            _nServiceBusOptions = nServiceBusOptions ?? new NServiceBusOptions();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var nameShortener = new RuleNameShortener();
            var endpointConfigurationRaw = RawEndpointConfiguration.Create(_attribute.Endpoint, OnMessage, _poisonMessageQueue);

            if (_nServiceBusOptions.EndpointConfiguration != null)
            {
                endpointConfigurationRaw = _nServiceBusOptions.EndpointConfiguration.Invoke(endpointConfigurationRaw);
            }
            else
            {
                var tokenProvider = TokenProvider.CreateManagedServiceIdentityTokenProvider();
                var transport = endpointConfigurationRaw.UseTransport<AzureServiceBusTransport>()                    
                .RuleNameShortener(nameShortener.Shorten)
                .CustomTokenProvider(tokenProvider)
                .ConnectionString(_attribute.Connection)
                .Transactions(TransportTransactionMode.ReceiveOnly);
            }

            if (!string.IsNullOrEmpty(NServiceBus.AzureFunction.Configuration.EnvironmentVariables.NServiceBusLicense))
            {
                endpointConfigurationRaw.UseLicense(NServiceBus.AzureFunction.Configuration.EnvironmentVariables.NServiceBusLicense);
            }
            endpointConfigurationRaw.DefaultErrorHandlingPolicy(_poisonMessageQueue, ImmediateRetryCount);
            endpointConfigurationRaw.AutoCreateQueue();

            _endpoint = await RawEndpoint.Start(endpointConfigurationRaw).ConfigureAwait(false);

            await _endpoint.SubscriptionManager.Subscribe(_parameter.ParameterType, new ContextBag());

        }

        protected async Task OnMessage(MessageContext context, IDispatchMessages dispatcher)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var triggerData = new TriggeredFunctionData
            {
                TriggerValue = new NServiceBusTriggerData
                {
                    Data = context.Body,
                    Headers = context.Headers,
                    Dispatcher = dispatcher
                }
            };

            if (_nServiceBusOptions.OnMessageReceived != null)
            {
                _nServiceBusOptions.OnMessageReceived.Invoke(context);
            }
            
            var result = await _executor.TryExecuteAsync(triggerData, _cancellationTokenSource.Token);

            if (!result.Succeeded)
            {
                if (_nServiceBusOptions.OnMessageErrored != null)
                {
                    _nServiceBusOptions.OnMessageErrored.Invoke(result.Exception, context);
                }

                throw result.Exception;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Cancel();
            return _endpoint.Stop();
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
        }
    }
}

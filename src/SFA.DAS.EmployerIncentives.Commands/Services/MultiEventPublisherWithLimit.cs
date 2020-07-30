using Polly;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class MultiEventPublisherWithLimit : IMultiEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly Policies _policies;

        public MultiEventPublisherWithLimit(
            IEventPublisher eventPublisher,
            Policies policies)
        {
            _eventPublisher = eventPublisher;
            _policies = policies;
        }

        public async Task Publish<T>(IEnumerable<T> messages) where T : class
        {
            var tasksToRun = new List<Task<PolicyResult>>();
            var policy = _policies.MultiEventPublishLimiterPolicy;

            foreach (var message in messages)
            {
                tasksToRun.Add(policy.ExecuteAndCaptureAsync((context) => _eventPublisher.Publish(message),
                    new Context("Event", new Dictionary<string, object> { { "Message", message } })
                ));
            }

            await Task.WhenAll(tasksToRun);

            var errors = new List<Exception>();
            foreach (var task in tasksToRun)
            {
                var result = await task;
                if (result.FinalException != null)
                {
                    errors.Add(new Exception($"Error publishing message '{typeof(T).Name}' with body {Newtonsoft.Json.JsonConvert.SerializeObject(result.Context["Message"]) }", result.FinalException));
                }
            }
            if (errors.Count > 0)
            {
                throw new AggregateException(errors);
            }
        }
    }
}

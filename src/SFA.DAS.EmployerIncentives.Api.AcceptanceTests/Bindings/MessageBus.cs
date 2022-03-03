using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "messageBus")]
    public class MessageBus
    {
        [BeforeScenario(Order = 4)]
        public Task InitialiseMessageBus(TestContext context)
        {
            context.MessageBus = new TestMessageBus(context);
            context.Hooks.Add(new Hook<MessageContext>());
            return context.MessageBus.Start();
        }

        [AfterScenario(Order = 4)]
        public async Task CleanUp(TestContext context)
        {
            if (context.MessageBus != null && context.MessageBus.IsRunning)
            {
                await context.MessageBus.Stop();
            }            
        }
    }
}

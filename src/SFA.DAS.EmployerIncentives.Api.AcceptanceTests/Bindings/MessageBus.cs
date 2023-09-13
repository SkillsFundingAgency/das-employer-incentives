using System.Threading.Tasks;
using TechTalk.SpecFlow;
using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "messageBus")]
    public class MessageBus
    {
        private readonly AcceptanceTests.TestContext _context;

        public MessageBus(AcceptanceTests.TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 2)]
        public Task InitialiseMessageBus()
        {
            _context.MessageBus = new TestMessageBus(_context);
             _context.Hooks.Add(new Hook<MessageContext>());
            return _context.MessageBus.Start();
        }

        [AfterScenario(Order = 2)]
        public async Task CleanUp()
        {
            Console.WriteLine($"TESTRUN: MessageBus CleanUp");
            if (_context.MessageBus != null && _context.MessageBus.IsRunning)
            {
                Console.WriteLine($"TESTRUN: MessageBus Stop");
                await _context.MessageBus.Stop();                
            }            
        }
    }
}

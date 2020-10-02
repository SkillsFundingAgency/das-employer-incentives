﻿using System.Threading.Tasks;
using TechTalk.SpecFlow;
using NServiceBus.Transport;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Bindings
{
    [Binding]
    [Scope(Tag = "messageBus")]
    public class MessageBus
    {
        private readonly TestContext _context;

        public MessageBus(TestContext context)
        {
            _context = context;
        }

        [BeforeScenario(Order = 2)]
        public Task InitialiseMessageBus()
        {
            _context.MessageBus = new TestMessageBus();
             _context.Hooks.Add(new Hook<MessageContext>());
            return _context.MessageBus.Start(_context.TestDirectory);
        }
    }
}

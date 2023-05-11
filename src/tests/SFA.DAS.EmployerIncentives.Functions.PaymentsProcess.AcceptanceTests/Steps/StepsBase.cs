using AutoFixture;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly Fixture Fixture;
        private static object _lock = new object();

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            Fixture = new Fixture();

            var commandsHook = testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as Hook<ICommand>;

            if (commandsHook != null)
            {
                commandsHook.OnReceived += (command) =>
                {
                    lock (_lock)
                    {
                        testContext.CommandsPublished.Add(
                        new PublishedCommand(command)
                        {
                            IsReceived = true,
                            IsDomainCommand = command is DomainCommand
                        });
                    }
                };

                commandsHook.OnProcessed += (command) =>
                {
                    lock (_lock)
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList().ForEach(c => c.IsProcessed = true);

                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterProcessedCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };

                commandsHook.OnPublished += (command) =>
                {
                    lock (_lock)
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList().ForEach(c => c.IsPublished = true);

                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };

                commandsHook.OnErrored += (ex, command) =>
                {
                    lock (_lock)
                    {
                        List<PublishedCommand> publishedCommands;
                        publishedCommands = testContext.CommandsPublished.Where(c => c.Command == command && c.IsDomainCommand == command is DomainCommand).ToList();

                        publishedCommands.ForEach(c =>
                        {
                            c.IsErrored = true;
                            c.LastError = ex;
                            if (ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
                            {
                                c.IsPublishedWithNoListener = true;
                            }
                        });

                        if (ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
                        {
                            return true;
                        }

                        return false;
                    }
                };
            }
        }
    }
}

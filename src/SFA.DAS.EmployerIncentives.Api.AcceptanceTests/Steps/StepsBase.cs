using AutoFixture;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;

//[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly EmployerIncentiveApi EmployerIncentiveApi;
        protected readonly Fixture Fixture;
        protected readonly DataAccess DataAccess;
        private static object _lock = new object();

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            EmployerIncentiveApi = testContext.EmployerIncentiveApi;
            Fixture = new Fixture();
            DataAccess = new DataAccess(testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var hook = testContext.Hooks.SingleOrDefault(h => h is Hook<object>) as Hook<object>;

            if (hook != null)
            {
                hook.OnProcessed = (message) =>
                {
                    lock (_lock)
                    {
                        testContext.EventsPublished.Add(message);
                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishEvent");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };
            }

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
                commandsHook.OnHandled += (command) =>
                {
                    lock (_lock)
                    {
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

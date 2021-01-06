using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly EmployerIncentiveApi EmployerIncentiveApi;
        protected readonly Fixture Fixture;
        protected readonly DataAccess DataAccess;

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
                    testContext.EventsPublished.Add(message);
                    var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishEvent");
                    if (throwError)
                    {
                        throw new ApplicationException("Unexpected exception, should force a rollback");
                    }
                };
            }

            var commandsHook = testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as Hook<ICommand>;

            if (commandsHook != null)
            {
                commandsHook.OnReceived += (command) =>
                {
                    if (command is DomainCommand)
                    {
                        testContext.DomainCommandsPublished.Add(new PublishedCommand(command) { IsReceived = true });
                    }
                    else
                    {
                        testContext.CommandsPublished.Add(new PublishedCommand(command) { IsReceived = true });
                    }
                };

                commandsHook.OnProcessed += (command) =>
                {
                    if (command is DomainCommand)
                    {
                        testContext.DomainCommandsPublished.Where(c => c.Command == command).ToList().ForEach(c => c.IsPublished = true);
                    }
                    else
                    {
                        testContext.CommandsPublished.Where(c => c.Command == command).ToList().ForEach(c => c.IsPublished = true);
                    }
                    var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishCommand");
                    if (throwError)
                    {
                        throw new ApplicationException("Unexpected exception, should force a rollback");
                    }
                };
                commandsHook.OnErrored += (ex, command) =>
                {
                    List<PublishedCommand> publishedCommands;
                    if (command is DomainCommand)
                    {
                        publishedCommands = testContext.DomainCommandsPublished.Where(c => c.Command == command).ToList();
                    }
                    else
                    {
                        publishedCommands = testContext.CommandsPublished.Where(c => c.Command == command).ToList();
                    }
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
                };
            }
        }
    }
}

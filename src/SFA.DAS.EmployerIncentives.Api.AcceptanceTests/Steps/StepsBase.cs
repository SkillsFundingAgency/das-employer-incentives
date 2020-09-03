using System;
using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly EmployerIncentiveApi EmployerIncentiveApi;
        protected readonly Fixture Fixture;

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            EmployerIncentiveApi = testContext.EmployerIncentiveApi;
            Fixture = new Fixture();

            var hook = testContext.Hooks.SingleOrDefault(h => h is Hook<object>) as Hook<object>;

            if (hook != null)
            {
                hook.OnProcessed = (message) =>
                {
                    testContext.EventsPublished.Add(message);
                    if (testContext.ThrowErrorAfterSendingEvent)
                    {
                        throw new ApplicationException("Unexpected exception, should force a rollback");
                    }
                };
            }

            var commandsHook = testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as Hook<ICommand>;

            if (commandsHook != null)
            {
                commandsHook.OnReceived = (command) =>
                {
                    testContext.CommandsPublished.Add(new PublishedCommand(command) { IsReceived = true } );
                };
                commandsHook.OnProcessed = (command) =>
                {
                    testContext.CommandsPublished.Single(c => c.Command == command).IsPublished = true;
                };
                commandsHook.OnErrored = (ex, command) =>
                {
                    var publishedCommand = testContext.CommandsPublished.Single(c => c.Command == command);
                    publishedCommand.IsErrored = true;
                    publishedCommand.LastError = ex;
                    if(ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
                    {
                        publishedCommand.IsPublishedWithNoListener = true;
                        return true;
                    }
                    return false;
                };
            }
        }
    }
}

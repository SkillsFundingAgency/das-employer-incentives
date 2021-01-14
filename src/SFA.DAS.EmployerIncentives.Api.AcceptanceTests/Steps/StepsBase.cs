using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using System;
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
                commandsHook.OnReceived = (command) =>
                {
                    lock (_lock)
                    {
                        testContext.CommandsPublished.Add(new PublishedCommand(command) { IsReceived = true });
                    }
                };
                commandsHook.OnProcessed = (command) =>
                {
                    lock (_lock)
                    {
                        testContext.CommandsPublished.Single(c => c.Command == command).IsPublished = true;
                        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishCommand");
                        if (throwError)
                        {
                            throw new ApplicationException("Unexpected exception, should force a rollback");
                        }
                    }
                };
                commandsHook.OnErrored = (ex, command) =>
                {
                    lock (_lock)
                    {
                        var publishedCommand = testContext.CommandsPublished.Single(c => c.Command == command);
                        publishedCommand.IsErrored = true;
                        publishedCommand.LastError = ex;
                        if (ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
                        {
                            publishedCommand.IsPublishedWithNoListener = true;
                            return true;
                        }
                        return false;
                    }
                };
            }
        }
    }
}

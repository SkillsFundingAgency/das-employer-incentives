using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using System.Linq;

[assembly: Parallelizable(ParallelScope.Fixtures)]
namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class StepsBase
    {
        protected readonly TestContext TestContext;
        protected readonly Fixture Fixture;
        protected readonly DataRepository DataRepository;

        public StepsBase(TestContext testContext)
        {
            TestContext = testContext;
            Fixture = new Fixture();
            DataRepository = new DataRepository(testContext.SqlDatabase.DatabaseInfo.ConnectionString);

            var hook = testContext.Hooks.SingleOrDefault(h => h is Hook<object>) as Hook<object>;

            //if (hook != null)
            //{
            //    hook.OnProcessed = (message) =>
            //    {
            //        testContext.EventsPublished.Add(message);
            //        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishEvent");
            //        if (throwError)
            //        {
            //            throw new ApplicationException("Unexpected exception, should force a rollback");
            //        }
            //    };
            //}

            //var commandsHook = testContext.Hooks.SingleOrDefault(h => h is Hook<ICommand>) as Hook<ICommand>;

            //if (commandsHook != null)
            //{
            //    commandsHook.OnReceived = (command) =>
            //    {
            //        testContext.CommandsPublished.Add(new PublishedCommand(command) { IsReceived = true });
            //    };
            //    commandsHook.OnProcessed = (command) =>
            //    {
            //        testContext.CommandsPublished.Single(c => c.Command == command).IsPublished = true;
            //        var throwError = testContext.TestData.Get<bool>("ThrowErrorAfterPublishCommand");
            //        if (throwError)
            //        {
            //            throw new ApplicationException("Unexpected exception, should force a rollback");
            //        }
            //    };
            //    commandsHook.OnErrored = (ex, command) =>
            //    {
            //        var publishedCommand = testContext.CommandsPublished.Single(c => c.Command == command);
            //        publishedCommand.IsErrored = true;
            //        publishedCommand.LastError = ex;
            //        if (ex.Message.Equals($"No destination specified for message: {command.GetType().FullName}"))
            //        {
            //            publishedCommand.IsPublishedWithNoListener = true;
            //            return true;
            //        }
            //        return false;
            //    };
            //}
        }
    }
}

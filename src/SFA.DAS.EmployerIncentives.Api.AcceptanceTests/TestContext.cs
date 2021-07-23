using SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.HashingService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests
{
    public class TestContext
    {
        public string InstanceId { get; private set; }
        public CancellationToken CancellationToken { get; set; }
        public DirectoryInfo TestDirectory { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public EmployerIncentiveApi EmployerIncentiveApi { get; set; }
        public TestAccountApi AccountApi { get; set; }
        public TestLearnerMatchApi LearnerMatchApi { get; set; }
        public TestMessageBus MessageBus { get; set; }
        public TestDomainMessageHandlers DomainMessageHandlers { get; set; }
        public TestData TestData { get; set; }
        public IHashingService HashingService { get; set; }
        public List<IHook> Hooks { get; set; }
        public List<object> EventsPublished { get; set; }
        public List<PublishedCommand> CommandsPublished { get; set; }
        public TestWebApi EmployerIncentivesWebApiFactory { get; set; }
        public Data.ApprenticeshipIncentives.Models.CollectionCalendarPeriod ActivePeriod { get; set; }
        public (string AllowedHashstringCharacters, string HashString) HashingServiceConfig = ("46789BCDFGHJKLMNPRSTVWXY", Guid.NewGuid().ToString());

        public TestContext()
        {
            InstanceId = Guid.NewGuid().ToString();
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName, $"TestDirectory/{InstanceId}"));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            TestData.GetOrCreate("ThrowErrorAfterPublishCommand", () => false);
            TestData.GetOrCreate("ThrowErrorAfterProcessedCommand", () => false);
            TestData.GetOrCreate("ThrowErrorAfterPublishEvent", () => false);
            HashingService = new HashingService.HashingService(HashingServiceConfig.AllowedHashstringCharacters, HashingServiceConfig.HashString);
            Hooks = new List<IHook>();
            EventsPublished = new List<object>();
            CommandsPublished = new List<PublishedCommand>();
        }
    }
}

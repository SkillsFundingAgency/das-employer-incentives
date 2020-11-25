using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public class TestContext
    {
        public string InstanceId { get; private set; }
        public DirectoryInfo TestDirectory { get; set; }
        public TestPaymentsProcessFunctions PaymentsProcessFunctions { get; set; }
        public TestData TestData { get; set; }
        public List<IHook> Hooks { get; set; }
        public SqlDatabase SqlDatabase { get; set; }
        public TestLearnerMatchApi LearnerMatchApi { get; set; }
        public Data.ApprenticeshipIncentives.Models.CollectionPeriod ActivePeriod { get; set; }

        public TestContext()
        {
            InstanceId = Guid.NewGuid().ToString();
            TestDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), InstanceId));
            if (!TestDirectory.Exists)
            {
                Directory.CreateDirectory(TestDirectory.FullName);
            }
            TestData = new TestData();
            Hooks = new List<IHook>();
        }
    }
}
using SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Hooks;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services;
using System;
using System.Collections.Generic;
using System.IO;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests
{
    public static class MockServers
    {
        public static MockApi PaymentsApi { get; set; }
        public static MockApi LearnerMatchApi { get; set; }
    }

    public class TestContext : IDisposable
    {
        public string InstanceId { get; private set; }
        public DirectoryInfo TestDirectory { get; set; }
        public TestFunction TestFunction { get; set; }

        public TestData TestData { get; set; }
        public List<IHook> Hooks { get; set; }
        public SqlDatabase SqlDatabase { get; set; }

        public MockApi LearnerMatchApi
        {
            get => MockServers.LearnerMatchApi;
            set => MockServers.LearnerMatchApi = value;
        }

        public MockApi PaymentsApi
        {
            get => MockServers.PaymentsApi;
            set => MockServers.PaymentsApi = value;
        }
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

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                LearnerMatchApi?.Reset();
                PaymentsApi?.Reset();
            }

            _isDisposed = true;
        }
    }
}
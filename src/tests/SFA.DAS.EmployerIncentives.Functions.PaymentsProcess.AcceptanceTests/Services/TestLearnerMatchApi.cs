using System;
using WireMock.Server;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Services
{
    public class TestLearnerMatchApi : IDisposable
    {
        private bool isDisposed;

        public string BaseAddress { get; private set; }

        public WireMockServer MockServer { get; private set; }

        public TestLearnerMatchApi()
        {
            MockServer = WireMockServer.Start();
            BaseAddress = MockServer.Urls[0];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                if (MockServer.IsStarted)
                {
                    MockServer.Stop();
                }
                MockServer.Dispose();
            }

            isDisposed = true;
        }
    }
}

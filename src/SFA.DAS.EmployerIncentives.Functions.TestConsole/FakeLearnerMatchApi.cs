using System;
using WireMock.Server;

namespace SFA.DAS.EmployerIncentives.Functions.TestConsole
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    public class FakeLearnerMatchApi : IDisposable
    {
        private readonly WireMockServer _server;
        public FakeLearnerMatchApi(WireMockServer server)
        {
            _server = server;
        }

        public void Dispose()
        {
            if (_server.IsStarted)
            {
                _server.Stop();
            }
        }
    }
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
}
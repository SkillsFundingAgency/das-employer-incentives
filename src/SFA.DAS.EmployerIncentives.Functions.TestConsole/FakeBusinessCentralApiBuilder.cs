using AutoFixture;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Diagnostics;
using System.Net;
using WireMock.Logging;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SFA.DAS.EmployerIncentives.Functions.TestConsole
{
    public class FakeBusinessCentralApiBuilder
    {

        private readonly WireMockServer _server;
        private readonly Fixture _fixture = new Fixture();

        public static FakeBusinessCentralApiBuilder Create(int port)
        {
            return new FakeBusinessCentralApiBuilder(port);
        }

        private FakeBusinessCentralApiBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port);
        }

        public WireMockServer Build()
        {
            _server.LogEntriesChanged += _server_LogEntriesChanged;
            return _server;
        }

        private void _server_LogEntriesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (LogEntry newItem in e.NewItems)
            {
                Debug.WriteLine("============================= FakeBusinessCentralApi MockServer called ================================");
                Debug.WriteLine(JsonConvert.SerializeObject(newItem), Formatting.Indented);
                Debug.WriteLine("==========================================================================================================");
            }
        }

        public FakeBusinessCentralApiBuilder WithPaymentRequestsEndpoint()
        {
            _server
                .Given(
                    Request
                        .Create()
                        .WithPath($"/payments/requests")
                        .UsingPost()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.Accepted));

            return this;
        }
    }
}
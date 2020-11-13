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
    public class FakeLearnerMatchApiBuilder
    {

        private readonly WireMockServer _server;
        private readonly Fixture _fixture = new Fixture();
        private object _learnerMatchApiData;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private Account _accountModel;

        public static FakeLearnerMatchApiBuilder Create(int port)
        {
            return new FakeLearnerMatchApiBuilder(port);
        }

        private FakeLearnerMatchApiBuilder(int port)
        {
            _server = WireMockServer.StartWithAdminInterface(port);
        }

        public FakeLearnerMatchApi Build()
        {
            _server.LogEntriesChanged += _server_LogEntriesChanged;
            return new FakeLearnerMatchApi(_server);
        }

        private void _server_LogEntriesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (LogEntry newItem in e.NewItems)
            {
                Debug.WriteLine("============================= TestLearnerApi MockServer called ================================");
                Debug.WriteLine(JsonConvert.SerializeObject(newItem), Formatting.Indented);
                Debug.WriteLine("==========================================================================================================");
            }
        }

        public FakeLearnerMatchApiBuilder WithLearnerMatchingApi()
        {
            _accountModel = _fixture.Create<Account>();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .Create();


            _learnerMatchApiData = _fixture.Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Learner, _fixture.Build<LearnerDto>().With(l => l.Uln, _apprenticeshipIncentive.ULN).Create())
                .Create();

            _server
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/*/*")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_learnerMatchApiData));

            return this;
        }
    }

}
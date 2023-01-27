using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ReinstatePayments")]
    public class ReinstatePaymentsSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private Data.ApprenticeshipIncentives.Models.Archive.PendingPayment _archivedPendingPayment;
        private HttpResponseMessage _response;
        private ReinstatePaymentsRequest _reinstatePaymentsRequest;

        public ReinstatePaymentsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
        }

        [Given(@"a pending payment has been archived for an apprenticeship incentive")]
        public async Task GivenAPendingPaymentHasBeenArchivedForAnApprenticeshipIncentive()
        {
            _apprenticeshipIncentive = _fixture.Create<ApprenticeshipIncentive>();
            _archivedPendingPayment = _fixture.Build<Data.ApprenticeshipIncentives.Models.Archive.PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                .Create();

            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_archivedPendingPayment);
        }

        [When(@"a reinstate request is received")]
        public async Task WhenAReinstateRequestIsReceived()
        {
            _reinstatePaymentsRequest = _fixture.Build<ReinstatePaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { _archivedPendingPayment.PendingPaymentId })
                .Create();

            var url = "reinstate-payments";

            _response = await EmployerIncentiveApi.Post(url, _reinstatePaymentsRequest);
        }

        [When(@"a reinstate request is received for a pending payment id that does not match")]
        public async Task WhenAReinstateRequestIsReceivedForAPendingPaymentIdThatDoesNotMatch()
        {
            _reinstatePaymentsRequest = _fixture.Build<ReinstatePaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { Guid.NewGuid() })
                .Create();

            var url = "reinstate-payments";

            _response = await EmployerIncentiveApi.Post(url, _reinstatePaymentsRequest);
        }

        [Then(@"the pending payment is reinstated")]
        public async Task ThenThePendingPaymentIsReinstated()
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>();
            var reinstatedPendingPayment = pendingPayments.FirstOrDefault(x => x.Id == _archivedPendingPayment.PendingPaymentId);
            reinstatedPendingPayment.Should().NotBeNull();
            reinstatedPendingPayment.ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            reinstatedPendingPayment.PaymentMadeDate.Should().BeNull();
        }

        [Then(@"a log is written for the reinstate action")]
        public async Task ThenALogIsWrittenForTheReinstateAction()
        {
            await using var dbConnection = new SqlConnection(_connectionString);

            var reinstatedPendingPaymentAudits = dbConnection.GetAll<ReinstatedPendingPaymentAudit>().ToList();
            reinstatedPendingPaymentAudits.Count.Should().Be(1);
            reinstatedPendingPaymentAudits[0].ApprenticeshipIncentiveId.Should().Be(_apprenticeshipIncentive.Id);
            reinstatedPendingPaymentAudits[0].PendingPaymentId.Should().Be(_archivedPendingPayment.PendingPaymentId);
        }

        [Then(@"an error is returned")]
        public void ThenAnErrorIsReturned()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

}
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{

    [Binding]
    [Scope(Feature = "RevertPayments")]
    public class RevertPaymentsSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly string _connectionString;
        private RevertPaymentsRequest _revertPaymentsRequest;
        private ApprenticeshipIncentive _apprenticeshipIncentive;
        private PendingPayment _pendingPayment;
        private Payment _payment;
        private List<ApprenticeshipIncentive> _apprenticeshipIncentives;
        private List<PendingPayment> _pendingPayments;
        private List<Payment> _payments;
        private HttpResponseMessage _response;

        public RevertPaymentsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _connectionString = _testContext.SqlDatabase.DatabaseInfo.ConnectionString;
        }

        [Given(@"an apprenticeship exists with a payment marked as paid")]
        public async Task GivenAnApprenticeshipExistsWithAPaymentMarkedAsPaid()
        {
            _apprenticeshipIncentive = _fixture.Create<ApprenticeshipIncentive>();
            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                .Create();
            _payment = _fixture.Build<Payment>()
                .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(x => x.PendingPaymentId, _pendingPayment.Id)
                .With(x => x.PaidDate, _fixture.Create<DateTime>())
                .Create();

            using var dbConnection = new SqlConnection(_connectionString);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_pendingPayment);
            await dbConnection.InsertAsync(_payment);
        }

        [Given(@"apprentice incentives exist with payments marked as paid")]
        public async Task GivenApprenticeshipIncentivesExistWithPaymentsMarkedAsPaid()
        {
            using var dbConnection = new SqlConnection(_connectionString);

            _apprenticeshipIncentives = new List<ApprenticeshipIncentive>();
            _pendingPayments = new List<PendingPayment>();
            _payments = new List<Payment>();
            
            for(var i = 0; i < 10; i++)
            {
                var apprenticeshipIncentive = _fixture.Create<ApprenticeshipIncentive>();
                _apprenticeshipIncentives.Add(apprenticeshipIncentive);

                var pendingPayment = _fixture.Build<PendingPayment>()
                    .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                    .With(x => x.PaymentMadeDate, _fixture.Create<DateTime>())
                    .Create();
                _pendingPayments.Add(pendingPayment);

                var payment = _fixture.Build<Payment>()
                    .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                    .With(x => x.PendingPaymentId, pendingPayment.Id)
                    .With(x => x.PaidDate, _fixture.Create<DateTime>())
                    .Create();
                _payments.Add(payment);

                await dbConnection.InsertAsync(apprenticeshipIncentive);
                await dbConnection.InsertAsync(pendingPayment);
                await dbConnection.InsertAsync(payment);
            }
        }

        [When(@"the revert payments request is sent for a single payment")]
        public async Task WhenTheRevertPaymentsRequestIsSentForASinglePayment()
        {
            _revertPaymentsRequest = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { _payment.Id })
                .Create();

            var url = "revert-payments";

            _response = await EmployerIncentiveApi.Post(url, _revertPaymentsRequest);
        }

        [When(@"the revert payments request is sent with an unmatching payment ID")]
        public async Task WhenTheRevertPaymentsRequestIsSentWithAnUnmatchingPaymentId()
        {
            _revertPaymentsRequest = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, new List<Guid> { Guid.NewGuid() })
                .Create();

            var url = "revert-payments";

            _response = await EmployerIncentiveApi.Post(url, _revertPaymentsRequest);
        }

        [When(@"the revert payments request is sent for multiple payments")]
        public async Task WhenTheRevertPaymentsRequestIsSentForMultiplePayments()
        {
            var paymentIds = _payments.Select(x => x.Id).ToList();

            _revertPaymentsRequest = _fixture.Build<RevertPaymentsRequest>()
                .With(x => x.Payments, paymentIds)
                .Create();

            var url = "revert-payments";

            _response = await EmployerIncentiveApi.Post(url, _revertPaymentsRequest);
        }

        [Then(@"the payment is reverted")]
        public async Task ThenThePaymentIsReverted()
        {
            _response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var content = await _response.Content.ReadAsStringAsync();
            JsonConvert.SerializeObject(content).Should().Contain("Payments have been successfully reverted");

            await using var dbConnection = new SqlConnection(_connectionString);
            var payments = dbConnection.GetAll<Payment>();

            payments.Count().Should().Be(0);

            var pendingPayments = dbConnection.GetAll<PendingPayment>();
            var revertedPayment = pendingPayments.FirstOrDefault(x => x.Id == _payment.PendingPaymentId);
            revertedPayment.PaymentMadeDate.Should().BeNull();
        }

        [Then(@"the payment is not reverted")]
        public async Task ThenThePaymentIsNotReverted()
        {
            await using var dbConnection = new SqlConnection(_connectionString);
            var payments = dbConnection.GetAll<Payment>();

            payments.Count().Should().Be(1);
            payments.FirstOrDefault(x => x.Id == _payment.Id).Should().NotBeNull();

            var pendingPayments = dbConnection.GetAll<PendingPayment>();
            var revertedPayment = pendingPayments.FirstOrDefault(x => x.Id == _payment.PendingPaymentId);
            revertedPayment.PaymentMadeDate.Should().NotBeNull();
        }

        [Then(@"the requester is informed no payment is found")]
        public async Task ThenTheRequesterIsInformedNoPaymentIsFound()
        {
            _response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            var content = await _response.Content.ReadAsStringAsync();
            JsonConvert.SerializeObject(content).Should().Contain($"Payment id {_revertPaymentsRequest.Payments[0]} not found");
        }

        [Then(@"the payments are reverted")]
        public async Task ThenThePaymensAresReverted()
        {
            _response.StatusCode.Should().Be((int)HttpStatusCode.OK);
            var content = await _response.Content.ReadAsStringAsync();
            JsonConvert.SerializeObject(content).Should().Contain("Payments have been successfully reverted");

            await using var dbConnection = new SqlConnection(_connectionString);
            var payments = dbConnection.GetAll<Payment>();

            payments.Count().Should().Be(0);

            var pendingPayments = dbConnection.GetAll<PendingPayment>();
            foreach(var payment in _payments)
            {
                var revertedPayment = pendingPayments.FirstOrDefault(x => x.Id == payment.PendingPaymentId);
                revertedPayment.PaymentMadeDate.Should().BeNull();
            }
        }
    }
}

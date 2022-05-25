using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "RetrospectiveBreakInLearning")]
    public class RetrospectiveBreakInLearningSteps
    {
        private DateTime _initialStartDate;
        private List<PendingPayment> _newEarnings;
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private readonly Account _accountModel;
        private readonly ApprenticeshipIncentive _apprenticeshipIncentive;
        private readonly PendingPayment _pendingPayment;
        private readonly Payment _payment;
        private List<EmploymentCheck> _employmentChecks;

        private DateTime _breakStart;
        private DateTime _breakEnd;
        private DateTime _incentiveEndDate;        

        protected RetrospectiveBreakInLearningSteps(TestContext context)
        {
            _testContext = context;
            _fixture = new Fixture();
            _accountModel = _fixture.Build<Account>()                
                .With(a => a.SignedAgreementVersion, Domain.ValueObjects.Phase2Incentive.MinimumAgreementVersion())
                .Create();

            _apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(p => p.DateOfBirth, DateTime.Now.AddYears(-24)) // under 25
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.HasPossibleChangeOfCircumstances, false)
                .With(p => p.RefreshedLearnerForEarnings, true)
                .With(p => p.PausePayments, false)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.BreakInLearnings, new List<ApprenticeshipBreakInLearning>())
                .With(p => p.Phase, Phase.Phase1)
                .With(p => p.MinimumAgreementVersion, Domain.ValueObjects.Phase2Incentive.MinimumAgreementVersion())
                .Create();

            _pendingPayment = _fixture.Build<PendingPayment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                .With(p => p.ClawedBack, false)
                .With(p => p.EarningType, EarningType.FirstPayment)
                .Without(p => p.PaymentMadeDate)
                .Create();

            _payment = _fixture.Build<Payment>()
                .With(p => p.AccountId, _accountModel.Id)
                .With(p => p.AccountLegalEntityId, _accountModel.AccountLegalEntityId)
                .With(p => p.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)                
                .With(p => p.PendingPaymentId, _pendingPayment.Id)
                .With(p => p.Amount, _pendingPayment.Amount)
                .With(p => p.PaymentPeriod, _pendingPayment.PeriodNumber)
                .With(p => p.PaymentYear, _pendingPayment.PaymentYear)
                .Without(p => p.CalculatedDate)
                .Without(p => p.PaidDate)                
                .Create();

            _employmentChecks = new List<EmploymentCheck>
            {
                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedAtStartOfApprenticeship)
                    .With(x => x.CreatedDateTime, DateTime.Now)
                    .With(x => x.Result, true)
                    .Create(),
                _fixture.Build<EmploymentCheck>()
                    .With(x => x.ApprenticeshipIncentiveId, _apprenticeshipIncentive.Id)
                    .With(x => x.CheckType, EmploymentCheckType.EmployedBeforeSchemeStarted)
                    .With(x => x.CreatedDateTime, DateTime.Now)
                    .With(x => x.Result, false)
                    .Create()
            };
        }

        [Given(@"an existing (.*) apprenticeship incentive with learning starting on (.*) and ending on (.*)")]
        public async Task GivenAnExistingApprenticeshipIncentiveWithLearningStartingIn_Oct(string phaseText, DateTime startDate, DateTime endDate)
        {
            _initialStartDate = startDate.Date;
            _incentiveEndDate = endDate.Date;
            _apprenticeshipIncentive.StartDate = startDate;
            _apprenticeshipIncentive.SubmittedDate = startDate.AddDays(-1);
            _apprenticeshipIncentive.Phase = Enum.Parse<Phase>(phaseText);
            
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_accountModel);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            await dbConnection.InsertAsync(_employmentChecks);
        }
                
        [Given(@"a payment of £(.*) is not sent in Period R(.*) (.*)")]
        public async Task GivenAPaymentIsNotSent(string payment, string period, string academicYear)
        {
            _pendingPayment.Amount = int.Parse(payment);
            _pendingPayment.PaymentYear = short.Parse(academicYear);
            _pendingPayment.PeriodNumber = byte.Parse(period);
            _pendingPayment.DueDate = _apprenticeshipIncentive.StartDate.AddDays(89);
            
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_pendingPayment);
        }

        [Given(@"a payment of £(.*) is sent in Period R(.*) (.*)")]
        public async Task GivenAPaymentIsSent(string payment, string period, string academicYear)
        {
            _pendingPayment.Amount = int.Parse(payment);
            _pendingPayment.PaymentYear = short.Parse(academicYear);
            _pendingPayment.PeriodNumber = byte.Parse(period);
            _pendingPayment.DueDate = _apprenticeshipIncentive.StartDate.AddDays(89);
            _pendingPayment.PaymentMadeDate = _pendingPayment.DueDate;

            _payment.Amount = _pendingPayment.Amount;
            _payment.CalculatedDate = DateTime.Now;
            _payment.PaidDate = _pendingPayment.DueDate;
            _payment.PaymentPeriod = _pendingPayment.PeriodNumber.Value;
            _payment.PaymentYear = _pendingPayment.PaymentYear.Value;

            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var existingPendingPayment = dbConnection.GetAll<PendingPayment>().Where(x => x.ApprenticeshipIncentiveId == _pendingPayment.ApprenticeshipIncentiveId &&  x.EarningType == _pendingPayment.EarningType).ToList();
            if(existingPendingPayment.Count == 0)
            {
                await dbConnection.InsertAsync(_pendingPayment);
            }
            else
            {
                await dbConnection.UpdateAsync(_pendingPayment);
            }
            
            await dbConnection.InsertAsync(_payment);
        }

        [Given(@"Learner data is updated with learning starting on (.*) and ending on (.*)")]
        public void GivenLearningIsUpdatedWithNoBreakInLearning(DateTime learningStartDate, DateTime learningEndDate)
        {
            var learnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, _pendingPayment.PaymentYear.ToString())
                .With(l => l.Training, new List<TrainingDto>
                    {
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, _pendingPayment.PaymentYear.ToString())
                                        .With(pe => pe.StartDate, learningStartDate)
                                        .With(pe => pe.EndDate, learningEndDate)
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber)
                                                .Create()
                                        })
                                        .Create()                                    
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            SetupMockLearnerMatchResponse(learnerMatchApiData);
        }

        [Given(@"Learner data is updated with a (.*) day Break in Learning before the first payment due date")]
        public void GivenABreakInLearningBeforeTheFirstPayment(int days)
        {
            var startDate = _pendingPayment.DueDate.AddDays(-2).Date;
            var endDate = startDate.AddDays(days).Date;
            SetupBreakInLearning(startDate, endDate);
        }

        [Given(@"Learner data is updated with a (.*) day Break in Learning before the first payment due date starting (.*)")]
        public void GivenABreakInLearningBeforeTheFirstPayment(int days, DateTime startDate)
        {
            var endDate = startDate.Date.AddDays(days).Date;
            SetupBreakInLearning(startDate.Date, endDate);
        }

        [Given(@"Learner data is updated with a (.*) day Break in Learning after the first payment due date")]
        public void GivenABreakInLearningAfterTheFirstPayment(int days)
        {
            var startDate = _pendingPayment.DueDate.AddDays(2).Date;
            var endDate = startDate.AddDays(days).Date;
            SetupBreakInLearning(startDate, endDate);
        }
        
        private void SetupBreakInLearning(DateTime start, DateTime end)
        {
            _breakStart = start;
            _breakEnd = end;

            var learnerMatchApiData = _fixture
                .Build<LearnerSubmissionDto>()
                .With(s => s.Ukprn, _apprenticeshipIncentive.UKPRN)
                .With(s => s.Uln, _apprenticeshipIncentive.ULN)
                .With(s => s.AcademicYear, _pendingPayment.PaymentYear.ToString())
                .With(l => l.Training, new List<TrainingDto>
                    {
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, _pendingPayment.PaymentYear.ToString())
                                        .With(pe => pe.StartDate, _initialStartDate)
                                        .With(pe => pe.EndDate, _breakStart.AddDays(-1))
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber)
                                                .Create()
                                        })
                                        .Create(),
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear,  _pendingPayment.PeriodNumber.Value == 12 ? NextAcademicYear(_pendingPayment.PaymentYear.Value).ToString() : _pendingPayment.PaymentYear.ToString())
                                        .With(pe => pe.StartDate, _breakEnd)
                                        .With(pe => pe.EndDate, _incentiveEndDate)
                                        .With(pe => pe.Periods, new List<PeriodDto>()
                                        {
                                            _fixture.Build<PeriodDto>()
                                                .With(period => period.ApprenticeshipId, _apprenticeshipIncentive.ApprenticeshipId)
                                                .With(period => period.IsPayable, true)
                                                .With(period => period.Period, _pendingPayment.PeriodNumber.Value == 12 ? 1 : (_pendingPayment.PeriodNumber.Value + 1))
                                                .Create()
                                        })

                                        .Create()
                                }
                            )
                            .Create()
                    }
                )
                .Create();

            SetupMockLearnerMatchResponse(learnerMatchApiData);
        }

        [Given(@"the Learner Match is run in Period R(.*) (.*)")]
        [When(@"the Learner Match is run in Period R(.*) (.*)")]
        public async Task WhenTheLearnerMatchIsRunInPeriodR(byte period, short year)
        {
            await _testContext.SetActiveCollectionCalendarPeriod(year, period);
            await StartLearnerMatching();
        }

        [Given(@"the Payment Run occurs")]
        public async Task ThePaymentProcessIsRun()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "IncentivePaymentOrchestrator_HttpStart",
                    nameof(IncentivePaymentOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/IncentivePaymentOrchestrator/{_testContext.ActivePeriod.AcademicYear}/{_testContext.ActivePeriod.PeriodNumber}"
                        },
                        ["collectionPeriodYear"] = _testContext.ActivePeriod.AcademicYear,
                        ["collectionPeriodNumber"] = _testContext.ActivePeriod.PeriodNumber
                    },
                    expectedCustomStatus: "WaitingForPaymentApproval"
                ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

            var orchestratorStartResponse = await _testContext.TestFunction.GetOrchestratorStartResponse();
            var status = await _testContext.TestFunction.GetStatus(orchestratorStartResponse.Id);
            status.CustomStatus.ToObject<string>().Should().Be("WaitingForPaymentApproval");

            await ApprovePayments(orchestratorStartResponse.Id);

            status = await _testContext.TestFunction.GetStatus(orchestratorStartResponse.Id);
            status.CustomStatus.ToObject<string>().Should().Be("PaymentProcessingCompleted");

            await ThenTheBreakInLearningIsRecorded();
        }

        private async Task ApprovePayments(string orchestratorInstanceId)
        {
            _testContext.PaymentsApi.MockServer
               .Given(
                   Request
                       .Create()
                       .WithPath($"/payments/requests")
                       .WithHeader("Content-Type", "application/payments-data")
                       .WithParam("api-version", "2020-10-01")
                       .UsingPost()
               )
               .RespondWith(Response.Create()
                   .WithStatusCode(HttpStatusCode.Accepted)
                   .WithHeader("Content-Type", "application/json"));

            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "PaymentApproval_HttpStart",
                    nameof(IncentivePaymentOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/approvePayments/{orchestratorInstanceId}"
                        },
                        ["instanceId"] = orchestratorInstanceId
                    }
                ));

            _testContext.TestFunction.LastResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [When(@"the earnings are recalculated")]
        [Then(@"the earnings are recalculated")]
        public void WhenTheEarningsAreRecalculated()
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            _newEarnings = dbConnection.GetAll<PendingPayment>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();
        }

        [Then(@"the Break in Learning is recorded")]
        public async Task ThenTheBreakInLearningIsRecorded()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count(b => b.EndDate != null).Should().Be(1);
            breaksInLearning.Single(b => b.EndDate != null).StartDate.Should().Be(_breakStart);
            breaksInLearning.Single(b => b.EndDate != null).EndDate.Should().Be(_breakEnd.AddDays(-1));
            breaksInLearning.Single(b => b.EndDate != null).CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
            breaksInLearning.Single(b => b.EndDate != null).UpdatedDate.Should().BeNull();
        }

        [Then(@"the Break in Learning is amended")]
        public async Task ThenTheBreakInLearningIsAmended()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count(b => b.EndDate != null).Should().Be(1);
            breaksInLearning.Single(b => b.EndDate != null).StartDate.Should().Be(_breakStart);
            breaksInLearning.Single(b => b.EndDate != null).EndDate.Should().Be(_breakEnd.AddDays(-1));
            breaksInLearning.Single(b => b.EndDate != null).CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
            breaksInLearning.Single(b => b.EndDate != null).UpdatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(2));
        }
        

        [Then(@"the Break in Learning is removed")]
        public async Task ThenTheBreakInLearningIsRemoved()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count.Should().Be(0);
        }

        [Then(@"no Break in Learning is recorded")]
        public async Task ThenNoBreakInLearningIsRecorded()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var breaksInLearning = dbConnection.GetAll<ApprenticeshipBreakInLearning>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            breaksInLearning.Count.Should().Be(0);
        }

        [Then(@"a new first pending payment of £(.*) is created for Period R(.*) (.*)")]
        public void ThenANewFirstPendingPaymentOfIsCreated(int amount, byte period, short year)
        {
            AssertPendingPayment(amount, period, year, EarningType.FirstPayment);
        }

        [Then(@"a new second pending payment of £(.*) is created for Period R(.*) (.*)")]
        public void ThenANewSecondPendingPaymentOfIsCreated(int amount, byte period, short year)
        {
            AssertPendingPayment(amount, period, year, EarningType.SecondPayment);
        }

        [Given(@"the first pending payment of £(.*) is paid in Period R(.*) (.*)")]
        public void ThenANewFirstPendingPaymentOfIsPaid(int amount, byte period, short year)
        {
            using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var pendingPayments = dbConnection.GetAll<PendingPayment>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();
            var payments = dbConnection.GetAll<Payment>()
               .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            var pp = pendingPayments.Single(x =>
               x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
               && x.EarningType ==  EarningType.FirstPayment
               && !x.ClawedBack);

            pp.Amount.Should().Be(amount);
            pp.PaymentMadeDate.Should().NotBeNull();

            var p = payments.Single(x =>
               x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
               && x.PendingPaymentId == pp.Id);

            p.PaymentPeriod.Should().Be(period);
            p.PaymentYear.Should().Be(year);
        }

        [Then(@"the first pending payment is not changed")]
        public void ThenTheFirstPendingPaymentIsNotChanged()
        {
            AssertPendingPayment(_pendingPayment.Amount , _pendingPayment.PeriodNumber.Value, _pendingPayment.PaymentYear.Value, EarningType.FirstPayment);
        }

        [Then(@"the second pending payment is not created")]
        public void ThenTheSecondPendingPaymentIsNotCreated()
        {
            _newEarnings.Count(x =>
                x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.EarningType == EarningType.SecondPayment
                && !x.ClawedBack).Should().Be(0);
        }

        [Then(@"the Learner is In Learning")]
        public async Task ThenTheLearnerIsInLearning()
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var learner = dbConnection.GetAll<Learner>()
                .Single(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id);

            learner.InLearning.Should().BeTrue();
        }

        private void AssertPendingPayment(decimal amount, byte period, short year, EarningType earningType)
        {
            var pp = _newEarnings.Single(x =>
                x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id
                && x.EarningType == earningType
                && !x.ClawedBack);

            pp.Amount.Should().Be(amount);
            pp.PaymentMadeDate.Should().BeNull();
            pp.PeriodNumber.Should().Be(period);
            pp.PaymentYear.Should().Be(year);
        }

        [Then(@"the paid earning of £(.*) is marked as requiring a clawback in Period R(.*) (.*)")]
        public async Task ThenAClawbackIsRecorded(int amount, byte period, short year)
        {
            await using var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString);
            var clawbacks = dbConnection.GetAll<ClawbackPayment>()
                .Where(x => x.ApprenticeshipIncentiveId == _apprenticeshipIncentive.Id).ToList();

            clawbacks.Count.Should().Be(1);
            var clawback = clawbacks.Single();
            clawback.Amount.Should().Be(amount * -1);
            clawback.CollectionPeriod.Should().Be(period);
            clawback.CollectionPeriodYear.Should().Be(year);
        }

        private void SetupMockLearnerMatchResponse(LearnerSubmissionDto learnerMatchApiData)
        {
            _testContext.LearnerMatchApi.MockServer.Reset();

            _testContext.LearnerMatchApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1.0/{_apprenticeshipIncentive.UKPRN}/{_apprenticeshipIncentive.ULN}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(learnerMatchApiData));
        }

        private async Task StartLearnerMatching()
        {
            await _testContext.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "LearnerMatchingOrchestrator_Start",
                    nameof(LearnerMatchingOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = $"/api/orchestrators/LearnerMatchingOrchestrator"
                        }
                    }
                ));
        }

        private short NextAcademicYear(short academicYear)
        {
            return short.Parse( 
                (int.Parse(academicYear.ToString().Substring(0, 2)) + 1).ToString() +
                (int.Parse(academicYear.ToString().Substring(2, 2)) + 1).ToString()
                );
        }
    }
}

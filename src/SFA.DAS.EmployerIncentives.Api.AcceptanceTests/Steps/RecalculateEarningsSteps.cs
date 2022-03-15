using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "RecalculateEarnings")]
    public class RecalculateEarningsSteps : StepsBase
    {
        private Fixture _fixture;
        private long _accountLegalEntityId;
        private long _uln;
        private RecalculateEarningsRequest _request;
        private TestContext _testContext;

        public RecalculateEarningsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _accountLegalEntityId = _fixture.Create<long>();
            _uln = _fixture.Create<long>();
        }

        [Given(@"an apprenticeship incentive exists with calculated earnings")]
        public async Task GivenAnApprenticeshipIncentiveExistsWithCalculatedEarnings()
        {
            var account = _fixture.Build<Account>()
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId)
                .Create();

            var apprenticeshipIncentive = _fixture.Build<ApprenticeshipIncentive>()
                .With(x => x.AccountLegalEntityId, _accountLegalEntityId)
                .With(x => x.ULN, _uln)
                .Create();

            var pendingPayment1 = _fixture.Build<PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                .With(x => x.EarningType, EarningType.FirstPayment)
                .Without(x => x.PaymentMadeDate)
                .Create();

            var pendingPayment2 = _fixture.Build<PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                .With(x => x.EarningType, EarningType.SecondPayment)
                .Without(x => x.PaymentMadeDate)
                .Create();

            await DataAccess.SetupAccount(account);
            await DataAccess.Insert(apprenticeshipIncentive);
            await DataAccess.Insert(pendingPayment1);
            await DataAccess.Insert(pendingPayment2);
        }

        [When(@"a request is made for the incentive's earnings to be recalculated")]
        public async Task WhenARequestIsMadeForAnIncentivesEarningsToBeRecalculated()
        {
            var url = "/earningsRecalculations";
            _request = new RecalculateEarningsRequest()
            {
                IncentiveLearnerIdentifiers = new List<IncentiveLearnerIdentifierDto>
                {
                    new IncentiveLearnerIdentifierDto { AccountLegalEntityId = _accountLegalEntityId, ULN = _uln }
                }
            };
            var apiResult = await EmployerIncentiveApi.Client.PostAsync(url, _request.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"the earnings are updated")]
        public void ThenTheEarningsAreUpdated()
        {
            _testContext.CommandsPublished.Where(c =>
                    c.IsPublished &&
                    c.Command is CalculateEarningsCommand)
                .Select(c => c.Command)
                .Count().Should().Be(1);
        }
    }
}

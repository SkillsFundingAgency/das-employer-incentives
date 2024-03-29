﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;
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
        private Guid _apprenticeshipIncentiveId;
        private RecalculateEarningsRequest _request;
        private TestContext _testContext;
        
        public RecalculateEarningsSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _accountLegalEntityId = _fixture.Create<long>();
            _uln = _fixture.Create<long>();
            _apprenticeshipIncentiveId = Guid.NewGuid();
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
                .With(x => x.Phase, Phase.Phase3)
                .With(x => x.Id, _apprenticeshipIncentiveId)
                .Create();

            var pendingPayment1 = _fixture.Build<PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                .With(x => x.EarningType, EarningType.FirstPayment)
                .With(x => x.CalculatedDate, new DateTime(2022, 01, 01))
                .Without(x => x.PaymentMadeDate)
                .Create();

            var pendingPayment2 = _fixture.Build<PendingPayment>()
                .With(x => x.ApprenticeshipIncentiveId, apprenticeshipIncentive.Id)
                .With(x => x.EarningType, EarningType.SecondPayment)
                .With(x => x.CalculatedDate, new DateTime(2022, 01, 01))
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
                IncentiveLearnerIdentifiers = new List<IncentiveLearnerIdentifier>
                {
                    new IncentiveLearnerIdentifier { AccountLegalEntityId = _accountLegalEntityId, ULN = _uln }
                }
            };
            var apiResult = await EmployerIncentiveApi.Client.PostAsync(url, _request.GetStringContent());

            apiResult.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Then(@"the earnings are updated")]
        public void ThenTheEarningsAreUpdated()
        {
            var pendingPayments = DataAccess.GetPendingPayments(_apprenticeshipIncentiveId);
            foreach(var payment in pendingPayments)
            {
                payment.CalculatedDate.Year.Should().Be(DateTime.Now.Year);
                payment.CalculatedDate.Month.Should().Be(DateTime.Now.Month);
                payment.CalculatedDate.Day.Should().Be(DateTime.Now.Day);
            }
        }
    }
}

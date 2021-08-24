using FluentAssertions;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using TechTalk.SpecFlow;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "ApprenticeshipIncentivesRequested")]
    public class ApprenticeshipIncentivesRequestedSteps : StepsBase
    {
        private List<ApprenticeshipIncentiveDto> _apiResponse;
        private Account _account;
        private ApprenticeshipIncentive _apprenticeshipIncentive;

        public ApprenticeshipIncentivesRequestedSteps(TestContext testContext) : base(testContext)
        {
        }

        [Given(@"an employer has submitted apprenticeship incentives")]
        public async Task GivenAnEmployerHasSubmittedApprenticeshipIncentives()
        {
            _account = TestContext.TestData.GetOrCreate<Account>();
            _apprenticeshipIncentive = TestContext.TestData.GetOrCreate<ApprenticeshipIncentive>();
            _apprenticeshipIncentive.AccountId = _account.Id;
            _apprenticeshipIncentive.AccountLegalEntityId = _account.AccountLegalEntityId;

            await SetupApprenticeshipIncentive();
        }

        [When(@"they retrieve apprenticeship incentives")]
        public async Task WhenTheEmployerRetrievesApprenticeshipIncentives()
        {
            var url = $"/accounts/{_account.Id}/legalentities/{_account.AccountLegalEntityId}/apprenticeshipIncentives";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<List<ApprenticeshipIncentiveDto>>(url);

            status.Should().Be(HttpStatusCode.OK);

            _apiResponse = data;
        }

        [Then(@"the apprenticeship incentives are returned")]
        public void ThenTheApprenticeshipIncnetivesAreReturned()
        {
            _apiResponse.Should().NotBeEmpty();
            var apprenticeshipIncentive = _apiResponse.First();
            
            apprenticeshipIncentive.FirstName.Should().Be(_apprenticeshipIncentive.FirstName);
            apprenticeshipIncentive.LastName.Should().Be(_apprenticeshipIncentive.LastName);
            apprenticeshipIncentive.CourseName.Should().Be(_apprenticeshipIncentive.CourseName);
            apprenticeshipIncentive.StartDate.Should().BeCloseTo(_apprenticeshipIncentive.StartDate);
            apprenticeshipIncentive.ApprenticeshipId.Should().Be(_apprenticeshipIncentive.ApprenticeshipId);
            apprenticeshipIncentive.Id.Should().Be(_apprenticeshipIncentive.Id);
            apprenticeshipIncentive.UKPRN.Should().Be(_apprenticeshipIncentive.UKPRN);
            apprenticeshipIncentive.ULN.Should().Be(_apprenticeshipIncentive.ULN);
        }

        private async Task SetupApprenticeshipIncentive()
        {
            await using var dbConnection = new SqlConnection(TestContext.SqlDatabase.DatabaseInfo.ConnectionString);
            await dbConnection.InsertAsync(_account);
            await dbConnection.InsertAsync(_apprenticeshipIncentive);
            _apprenticeshipIncentive.PendingPayments = _apprenticeshipIncentive.PendingPayments.Take(2).ToList();
            _apprenticeshipIncentive.PendingPayments.First().EarningType = EarningType.FirstPayment;
            _apprenticeshipIncentive.PendingPayments.First().DueDate = new DateTime(2020, 1, 12);
            _apprenticeshipIncentive.PendingPayments.Last().EarningType = EarningType.SecondPayment;
            _apprenticeshipIncentive.PendingPayments.Last().DueDate = new DateTime(2020, 9, 11);

            foreach (var pendingPayment in _apprenticeshipIncentive.PendingPayments)
            {
                pendingPayment.DueDate = pendingPayment.DueDate.Date;
                pendingPayment.ApprenticeshipIncentiveId = _apprenticeshipIncentive.Id;
                await dbConnection.InsertWithEnumAsStringAsync(pendingPayment);
            }
        }
    }
}

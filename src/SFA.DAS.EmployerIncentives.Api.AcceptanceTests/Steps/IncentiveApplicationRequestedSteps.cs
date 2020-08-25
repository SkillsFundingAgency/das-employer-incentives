﻿using System.Data.SqlClient;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Data.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "IncentiveApplicationRequested")]
    public class IncentiveApplicationRequestedSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private Fixture _fixture;
        private CreateIncentiveApplicationRequest _request;
        private IncentiveApplicationDto _returnedApplication;
        private Account _testAccountTable;

        public IncentiveApplicationRequestedSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _testAccountTable = testContext.TestData.GetOrCreate<Account>();

            _fixture = new Fixture();
            _request = _fixture.Build<CreateIncentiveApplicationRequest>().With(x => x.AccountLegalEntityId, _testAccountTable.AccountLegalEntityId).Create();
        }

        [Given(@"An employer has selected the apprenticeships for their application")]
        public async Task GivenAnEmployerHasSelectedApprenticeships()
        {
            using (var dbConnection = new SqlConnection(_testContext.SqlDatabase.DatabaseInfo.ConnectionString))
            {
                await dbConnection.ExecuteAsync("insert into Accounts(id, accountLegalEntityId, legalEntityId, legalentityName) values(@id, @accountLegalEntityId, @legalEntityId, @legalentityName)", _testAccountTable);
            }

            var url = $"applications";
            await EmployerIncentiveApi.Post(url, _request);
        }

        [When(@"They retrieve the application")]
        public async Task WhenTheyRetrieveTheApplication()
        {
            var url = $"/accounts/{_request.AccountId}/applications/{_request.IncentiveApplicationId}";
            var (status, data) =
                await EmployerIncentiveApi.Client.GetValueAsync<IncentiveApplicationDto>(url);

            _returnedApplication = data;
        }

        [Then(@"the selected apprenticeships are returned")]
        public void ThenTheSavedApplicationIsReturned()
        {
            _returnedApplication.Should().NotBeNull();
            _returnedApplication.Apprenticeships.Count().Should().Be(_request.Apprenticeships.Count());
        }
    }
}

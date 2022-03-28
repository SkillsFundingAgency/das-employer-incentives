using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentivesForAccountLegalEntity;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.ApprenticeshipIncentive
{
    public class WhenGetApprenticeshipIncentives
    {
        private ApprenticeshipIncentiveQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new ApprenticeshipIncentiveQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_data_is_returned_for_existing_account()
        {
            // Arrange
            const long accountId = 7;
            const long accountLegalEntityId = 19;
            var expected = new GetApprenticeshipIncentivesForAccountLegalEntityResponse(_fixture.CreateMany<DataTransferObjects.Queries.ApprenticeshipIncentives.ApprenticeshipIncentive>().ToList());

            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipIncentivesForAccountLegalEntityRequest, GetApprenticeshipIncentivesForAccountLegalEntityResponse>(
                    It.Is<GetApprenticeshipIncentivesForAccountLegalEntityRequest>(r => r.AccountId == accountId && r.AccountLegalEntityId == accountLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetApprenticeshipIncentives(accountId, accountLegalEntityId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.ApprenticeshipIncentives);
        }

        [Test]
        public async Task Then_error_returned_for_non_existing_account()
        {
            // Arrange
            const long accountId = 7;
            const long accountLegalEntityId = 19;
            var expected = new GetApprenticeshipIncentivesForAccountLegalEntityResponse(new List<DataTransferObjects.Queries.ApprenticeshipIncentives.ApprenticeshipIncentive>());

            _queryDispatcherMock.Setup(x => x.Send<GetApprenticeshipIncentivesForAccountLegalEntityRequest, GetApprenticeshipIncentivesForAccountLegalEntityResponse>(
                    It.Is<GetApprenticeshipIncentivesForAccountLegalEntityRequest>(r => r.AccountId == accountId && r.AccountLegalEntityId == accountLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetApprenticeshipIncentives(accountId, accountLegalEntityId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}
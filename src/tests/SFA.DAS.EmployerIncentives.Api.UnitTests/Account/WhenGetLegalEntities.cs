using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntities;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenGetLegalEntities
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_data_is_returned_for_existing_account()
        {
            // Arrange
            const long accountId = 7;
            var expected = new GetLegalEntitiesResponse { LegalEntities = _fixture.CreateMany<LegalEntityDto>() };

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(
                    It.Is<GetLegalEntitiesRequest>(r => r.AccountId == accountId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntities(accountId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.LegalEntities);
        }

        [Test]
        public async Task Then_error_returned_for_non_existing_account()
        {
            // Arrange
            const long accountId = 7;
            var expected = new GetLegalEntitiesResponse();

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(
                    It.Is<GetLegalEntitiesRequest>(r => r.AccountId == accountId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntities(accountId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}
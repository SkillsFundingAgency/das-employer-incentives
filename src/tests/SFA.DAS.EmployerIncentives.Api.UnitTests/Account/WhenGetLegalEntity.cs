using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLegalEntity;
using SFA.DAS.EmployerIncentives.DataTransferObjects;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenGetLegalEntity
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
        public async Task Then_data_is_returned_for_existing_legal_entity()
        {
            // Arrange
            const long accountId = 6;
            const long accountLegalEntityId = 7;
            var expected = new GetLegalEntityResponse { LegalEntity = _fixture.Create<LegalEntity>() };

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntityRequest, GetLegalEntityResponse>(
                    It.Is<GetLegalEntityRequest>(r => r.AccountId == accountId && r.AccountLegalEntityId == accountLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntity(accountId, accountLegalEntityId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.LegalEntity);
        }

        [Test]
        public async Task Then_error_returned_for_non_existing_account()
        {
            // Arrange
            const long accountId = 6;
            const long accountLegalEntityId = 7;
            var expected = new GetLegalEntityResponse();

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntityRequest, GetLegalEntityResponse>(
                    It.Is<GetLegalEntityRequest>(r => r.AccountId == accountId && r.AccountLegalEntityId == accountLegalEntityId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntity(accountId, accountLegalEntityId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}
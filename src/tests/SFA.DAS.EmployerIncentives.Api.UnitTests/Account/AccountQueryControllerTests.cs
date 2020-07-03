using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class AccountQueryControllerTests
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();

            _sut = new AccountQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task When_GetLegalEntities_requested_Then_data_is_returned()
        {
            // Arrange
            const long accountId = 7;
            var expected = new GetLegalEntitiesResponse();

            _queryDispatcherMock.Setup(x => x.Send<GetLegalEntitiesRequest, GetLegalEntitiesResponse>(
                    It.Is<GetLegalEntitiesRequest>(r => r.AccountId == accountId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetLegalEntities(accountId);

            // Assert
            actual.Should().Be(expected);
        }

    }
}
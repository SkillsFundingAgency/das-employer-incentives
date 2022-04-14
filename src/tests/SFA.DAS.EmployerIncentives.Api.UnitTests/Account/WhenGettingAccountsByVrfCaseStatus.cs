using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.DataTransferObjects;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.EmployerIncentives.Queries.Account.GetAccountsWithVrfStatus;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenGettingAccountsByVrfCaseStatus
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
        public async Task Then_data_is_returned_for_matching_accounts()
        {
            // Arrange
            var vrfStatus = LegalEntityVrfCaseStatus.RejectedVerification;
            var expected = new GetAccountsWithVrfCaseStatusResponse { Accounts = _fixture.CreateMany<DataTransferObjects.Account>() };

            _queryDispatcherMock.Setup(x => x.Send<GetAccountsWithVrfCaseStatusRequest, GetAccountsWithVrfCaseStatusResponse>(
                    It.Is<GetAccountsWithVrfCaseStatusRequest>(r => r.VrfStatus == vrfStatus)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetAccountsWithVrfCaseStatus(vrfStatus) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.Accounts);
        }

    }
}

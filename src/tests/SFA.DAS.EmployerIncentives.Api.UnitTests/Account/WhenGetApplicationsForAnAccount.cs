using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenGetApplicationsForAnAccount
    {
        private AccountQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _queryDispatcher = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountQueryController(_queryDispatcher.Object);
        }

        [Test]
        public async Task Then_a_list_of_applications_and_apprenticeships_is_returned()
        {
            // Arrange
            var accountId = _fixture.Create<long>();

            var apprenticeApplicationList = new GetApplicationsResponse
            {
                ApprenticeApplications = _fixture.CreateMany<ApprenticeApplicationDto>()
            };

            _queryDispatcher.Setup(x => x.Send<GetApplicationsRequest, GetApplicationsResponse>(
                It.Is<GetApplicationsRequest>(y => y.AccountId == accountId)))
                .ReturnsAsync(apprenticeApplicationList);

            // Act
            var result = await _sut.GetApplications(accountId) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.Value.Should().Be(apprenticeApplicationList.ApprenticeApplications);
        }

        [Test]
        public async Task Then_an_error_is_returned_for_non_existing_account()
        {
            // Arrange
            var accountId = _fixture.Create<long>();
            var apprenticeApplicationList = new GetApplicationsResponse();

            _queryDispatcher.Setup(x => x.Send<GetApplicationsRequest, GetApplicationsResponse>(
                It.Is<GetApplicationsRequest>(y => y.AccountId == accountId)))
                .ReturnsAsync(apprenticeApplicationList);

            // Act
            var actual = await _sut.GetApplications(accountId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}

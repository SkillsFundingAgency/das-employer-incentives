using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    public class WhenGettingAnApplication
    {
        private ApplicationQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new ApplicationQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_the_application_is_returned()
        {
            // Arrange
            const long accountId = 7;
            var applicationId = Guid.NewGuid();
            var expected = new GetApplicationResponse(_fixture.Create<IncentiveApplicationDto>());

            _queryDispatcherMock.Setup(x => x.Send<GetApplicationRequest, GetApplicationResponse>(
                    It.Is<GetApplicationRequest>(r => r.AccountId == accountId && r.ApplicationId == applicationId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetApplication(accountId, applicationId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.Application);
        }

        [Test]
        public async Task Then_error_returned_when_the_account_does_not_exist()
        {
            // Arrange
            const long accountId = 7;
            var applicationId = Guid.NewGuid();
            var expected = new GetApplicationResponse(null);

            _queryDispatcherMock.Setup(x => x.Send<GetApplicationRequest, GetApplicationResponse>(
                    It.Is<GetApplicationRequest>(r => r.AccountId == accountId && r.ApplicationId == applicationId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetApplication(accountId, applicationId) as NotFoundResult;

            // Assert
            actual.Should().NotBeNull();
        }

    }
}
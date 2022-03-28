using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Application
{
    [TestFixture]
    public class WhenGettingTheLegalEntityForAnApplication
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
        public async Task Then_the_legal_entity_id_for_the_application_is_returned()
        {
            // Arrange
            var accountId = _fixture.Create<long>();
            var applicationId = Guid.NewGuid();
            var expected = new GetApplicationResponse(_fixture.Create<IncentiveApplication>());

            _queryDispatcherMock.Setup(x => x.Send<GetApplicationRequest, GetApplicationResponse>(
                    It.Is<GetApplicationRequest>(r => r.AccountId == accountId && r.ApplicationId == applicationId)))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetApplicationAccountLegalEntity(accountId, applicationId) as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected.Application.AccountLegalEntityId);
        }
    }
}
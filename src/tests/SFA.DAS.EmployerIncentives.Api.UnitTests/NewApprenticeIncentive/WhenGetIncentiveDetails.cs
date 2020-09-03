using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetIncentiveDetails;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.NewApprenticeIncentive
{
    public class WhenGetIncentiveDetails
    {
        private NewApprenticeIncentiveQueryController _sut;
        private Mock<IQueryDispatcher> _queryDispatcherMock;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _queryDispatcherMock = new Mock<IQueryDispatcher>();
            _fixture = new Fixture();
            _sut = new NewApprenticeIncentiveQueryController(_queryDispatcherMock.Object);
        }

        [Test]
        public async Task Then_incentive_data_is_returned()
        {
            // Arrange
            var expected = _fixture.Create<GetIncentiveDetailsResponse>();

            _queryDispatcherMock.Setup(x => x.Send<GetIncentiveDetailsRequest, GetIncentiveDetailsResponse>(It.IsAny<GetIncentiveDetailsRequest>()))
                .ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetIncentiveDetails() as OkObjectResult;

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(expected);
        }
    }
}
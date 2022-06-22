using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Queries.Account.GetLatestVendorRegistrationCaseUpdateDateTime;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    public class WhenGetLatestVendorRegistrationCaseUpdateDateTime
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
        public async Task Then_data_is_returned_from_the_query()
        {
            // Arrange
            var expected = new GetLatestVendorRegistrationCaseUpdateDateTimeResponse { LastUpdateDateTime = _fixture.Create<DateTime>() };

            _queryDispatcherMock.Setup(x => x.Send<GetLatestVendorRegistrationCaseUpdateDateTimeRequest, GetLatestVendorRegistrationCaseUpdateDateTimeResponse>(
                    It.IsAny<GetLatestVendorRegistrationCaseUpdateDateTimeRequest>())).ReturnsAsync(expected);

            // Act
            var actual = await _sut.GetAccounts( new Types.AccountFilter {OrderBy = "VrfCaseStatusLastUpdatedDateTime" }) as OkObjectResult;

            // Assert
            actual?.Value.Should().NotBeNull();
            if (actual != null) ((GetLatestVendorRegistrationCaseUpdateDateTimeResponse)actual.Value).LastUpdateDateTime.Should().Be(expected.LastUpdateDateTime);
        }

    }
}
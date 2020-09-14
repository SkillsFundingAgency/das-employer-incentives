using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Services.NewApprenticeIncentiveEligibilityService
{
    [TestFixture]
    public class WhenEligibleApprenticeshipsAreRequested
    {
        private Mock<IUlnValidationService> _ulnValidationServiceMock;
        private Domain.Services.NewApprenticeIncentiveEligibilityService _sut;

        [SetUp]
        public void SetUp()
        {
            _ulnValidationServiceMock = new Mock<IUlnValidationService>();
            _sut = new Domain.Services.NewApprenticeIncentiveEligibilityService(_ulnValidationServiceMock.Object, new Mock<ILogger<Domain.Services.NewApprenticeIncentiveEligibilityService>>().Object);
        }

        [Test]
        public async Task Then_an_eligible_apprenticeship_where_uln_not_applied_previously_returns_true()
        {
            var apprenticeship = new ApprenticeshipBuilder().WithValidIncentiveProperties().Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }

        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        public async Task Then_an_eligible_apprenticeship_returns_expected_result(bool isApproved, bool ulnPreviouslyApplied, bool expected)
        {
            _ulnValidationServiceMock.Setup(x => x.UlnAlreadyOnSubmittedIncentiveApplication(It.IsAny<long>()))
                .ReturnsAsync(ulnPreviouslyApplied);

            var apprenticeship = new ApprenticeshipBuilder().WithValidIncentiveProperties().WithIsApproved(isApproved).Build();

            var result = await _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().Be(expected);
        }
    }
}

using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Services.NewApprenticeIncentiveEligibilityService
{
    [TestFixture]
    public class WhenEligibleApprenticeshipsAreRequested
    {
        private Domain.Services.NewApprenticeIncentiveEligibilityService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new Domain.Services.NewApprenticeIncentiveEligibilityService();
        }

        [Test]
        public void Then_an_ineligible_apprenticeship_returns_false()
        {
            var apprenticeship = new ApprenticeshipBuilder().WithIsApproved(false).Build();

            var result = _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }

        [Test]
        public void Then_an_eligible_apprenticeship_returns_true()
        {
            var apprenticeship = new ApprenticeshipBuilder().WithValidIncentiveProperties().Build();

            var result = _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }
    }
}

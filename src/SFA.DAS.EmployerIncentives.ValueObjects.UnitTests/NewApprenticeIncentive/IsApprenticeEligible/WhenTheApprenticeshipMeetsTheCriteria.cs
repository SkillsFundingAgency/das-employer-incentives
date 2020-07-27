using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.ValueObjects.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenTheApprenticeshipMeetsTheCriteria
    {
        private ValueObjects.NewApprenticeIncentive _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ValueObjects.NewApprenticeIncentive();
        }

        [Test]
        public void Then_the_apprenticeship_is_eligible()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithValidIncentiveProperties()
                .Build();

            var result = _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }
    }
}

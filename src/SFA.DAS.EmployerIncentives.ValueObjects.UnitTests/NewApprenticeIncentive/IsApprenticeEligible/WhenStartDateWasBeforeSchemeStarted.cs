using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.ValueObjects.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenStartDateWasBeforeSchemeStarted
    {
        private ValueObjects.NewApprenticeIncentive _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ValueObjects.NewApprenticeIncentive();
        }

        [Test]
        public void Then_the_apprenticeship_is_not_eligible()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithStartDate(new DateTime(2020, 7, 31))
                .Build();

            var result = _sut.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }
    }
}

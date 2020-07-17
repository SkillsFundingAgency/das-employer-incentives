using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenStartDateWasBeforeSchemeStarted
    {
        [Test]
        public void Then_the_apprenticeship_is_not_eligible()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithStartDate(new DateTime(2020, 7, 31))
                .Build();

            var result = Domain.NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }
    }
}

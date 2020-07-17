using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.UnitTests.Builders.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenTheApprenticeshipMeetsTheCriteria
    {
        [Test]
        public void Then_the_apprenticeship_is_eligible()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithStartDate(new DateTime(2020, 8, 1))
                .WithIsApproved(true)
                .Build();

            var result = Domain.NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeTrue();
        }
    }
}

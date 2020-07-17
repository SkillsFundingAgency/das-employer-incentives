using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.UnitTests.Builders.ValueObjects;
using SFA.DAS.EmployerIncentives.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.NewApprenticeIncentive.IsApprenticeEligible
{
    [TestFixture]
    public class WhenTheApprenticeshipIsNotApproved
    {
        [Test]
        public void Then_the_apprenticeship_is_not_eligible()
        {
            var apprenticeship = new ApprenticeshipBuilder()
                .WithIsApproved(false)
                .Build();

            var result = Domain.NewApprenticeIncentive.NewApprenticeIncentive.IsApprenticeshipEligible(apprenticeship);

            result.Should().BeFalse();
        }
    }
}

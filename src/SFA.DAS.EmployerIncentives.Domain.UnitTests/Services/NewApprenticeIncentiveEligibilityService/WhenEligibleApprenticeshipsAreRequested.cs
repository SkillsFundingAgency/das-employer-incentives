using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.UnitTests.Builders.ValueObjects;
using SFA.DAS.EmployerIncentives.ValueObjects;

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
        public void Then_any_apprentices_which_dont_meet_the_incentive_criteria_are_filtered_out()
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new ApprenticeshipBuilder().WithValidIncentiveProperties().Build(),
                new ApprenticeshipBuilder().WithIsApproved(false).Build(),
                new ApprenticeshipBuilder().WithValidIncentiveProperties().Build()
            };

            var result = _sut.GetEligibileApprenticeships(apprenticeships);

            result.Count().Should().Be(2);
            result.Should().Contain(apprenticeships[0]);
            result.Should().Contain(apprenticeships[2]);
        }
    }
}

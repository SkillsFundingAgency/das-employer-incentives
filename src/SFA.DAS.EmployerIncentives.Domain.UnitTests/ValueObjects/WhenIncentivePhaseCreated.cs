using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenIncentivePhaseCreated
    {  

        [TestCase(Phase.NotSet)]
        [TestCase(Phase.Phase1_0)]
        [TestCase(Phase.Phase1_1)]
        [TestCase(Phase.Phase2_0)]
        public void Then_the_incentive_phase_is_initialised_when_created(Phase phase)
        {
            // Act
            var newIncentivePhase = new IncentivePhase(phase);

            // Assert
            newIncentivePhase.Identifier.Should().Be(phase);
        }

        [TestCase("2020-8-1", "2021-6-1", Phase.NotSet)]
        [TestCase("2020-8-1", "2021-5-31", Phase.Phase1_0)]
        [TestCase("2020-8-2", "2021-5-31", Phase.Phase1_0)]
        [TestCase("2021-1-31", "2021-5-31", Phase.Phase1_0)]
        [TestCase("2021-2-1", "2021-5-31", Phase.Phase1_1)]
        [TestCase("2021-2-2", "2021-5-31", Phase.Phase1_1)]
        [TestCase("2021-3-31", "2021-5-31", Phase.Phase1_1)]
        [TestCase("2021-4-1", "2021-5-31", Phase.NotSet)]
        public void Then_the_incentive_phase_is_created_by_the_factory_method(DateTime startDate, DateTime applicationSubmissionDate, Phase expectedPhaseId)
        {
            // Arrange
            var expectedPhase = new IncentivePhase(expectedPhaseId);

            // Act
            var result = IncentivePhase.Create(startDate, applicationSubmissionDate);

            // Assert
            result.Should().BeEquivalentTo(expectedPhase);
        }
    }
}

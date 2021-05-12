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
        [TestCase(Phase.Phase1)]
        [TestCase(Phase.Phase2)]
        public void Then_the_incentive_phase_is_initialised_when_created(Phase phase)
        {
            // Act
            var newIncentivePhase = new IncentivePhase(phase);

            // Assert
            newIncentivePhase.Identifier.Should().Be(phase);
        }

        [TestCase("2021-6-1", Phase.NotSet)]
        [TestCase("2021-5-31", Phase.Phase1)]
        public void Then_the_incentive_phase_is_created_by_the_factory_method(DateTime applicationSubmissionDate, Phase expectedPhaseId)
        {
            // Arrange
            var expectedPhase = new IncentivePhase(expectedPhaseId);

            // Act
            var result = IncentivePhase.Create(applicationSubmissionDate);

            // Assert
            result.Should().BeEquivalentTo(expectedPhase);
        }
    }
}

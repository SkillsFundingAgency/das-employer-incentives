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

        [Test()]
        public void Then_the_current_incentive_phase_is_created_by_the_factory_method()
        {
            // Arrange
            var expectedPhase = new IncentivePhase(Phase.Phase2);

            // Act
            var result = IncentivePhase.Create();

            // Assert
            result.Should().BeEquivalentTo(expectedPhase);
        }
    }
}

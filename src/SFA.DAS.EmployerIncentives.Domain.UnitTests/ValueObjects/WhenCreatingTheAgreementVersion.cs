using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenCreatingTheAgreementVersion
    {
        [TestCase(Phase.Phase1, 2020, 8, 1, 4)]
        [TestCase(Phase.Phase1, 2021, 1, 31, 4)]
        [TestCase(Phase.Phase1, 2021, 2, 1, 5)]
        [TestCase(Phase.Phase1, 2021, 3, 31, 5)]
        [TestCase(Phase.Phase2, 2021, 6, 1, 6)]
        [TestCase(Phase.Phase3, 2021, 10, 1, 7)]
        [TestCase(Phase.Phase3, 2022, 1, 31, 7)]

        public void Then_the_minimum_agreement_version_is_set(Phase phase, int year, int month, int day, int minimumAgreementVersion)
        {
            // Arrange
            var plannedStartDate = new DateTime(year, month, day);
            // Act
            var agreementVersion = AgreementVersion.Create(phase, plannedStartDate);

            // Assert
            agreementVersion.MinimumRequiredVersion.Should().Be(minimumAgreementVersion);
        }
    }
}

using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenChangingTheStartDateOfAnAgreementVersion
    {
        [TestCase(1, 2021, 6, 1, 4)]
        [TestCase(4, 2021, 6, 1, 4)]
        [TestCase(5, 2021, 6, 1, 5)]
        [TestCase(1, 2020, 8, 1, 4)]
        [TestCase(1, 2021, 1, 31, 4)]
        [TestCase(1, 2021, 2, 1, 5)]
        [TestCase(1, 2021, 3, 31, 5)]
        [TestCase(1, 2021, 5, 31, 5)]
        [TestCase(5, 2020, 8, 1, 5)]
        [TestCase(5, 2021, 1, 31, 5)]
        [TestCase(5, 2021, 2, 1, 5)]
        public void Then_the_minimum_agreement_version_is_set(
            int existingMinimumAgreementVersion, 
            int newYear, int newMonth, int newDay, 
            int minimumAgreementVersion)
        {
            // Arrange
            var agreementVersion = new AgreementVersion(existingMinimumAgreementVersion);
            var newStartDate = new DateTime(newYear, newMonth, newDay);

            // Act
            var newAgreementVersion = agreementVersion.ChangedStartDate(Phase.Phase1, newStartDate);

            // Assert
            newAgreementVersion.MinimumRequiredVersion.Should().Be(minimumAgreementVersion);
        }

        [Test]
        public void Then_the_minimum_agreement_isnt_updated_for_phase2()
        {
            var agreementVersion = new AgreementVersion(6);
            var newStartDate = new DateTime(2020, 8, 1);

            // Act
            var newAgreementVersion = agreementVersion.ChangedStartDate(Phase.Phase2, newStartDate);

            // Assert
            newAgreementVersion.Should().Be(agreementVersion);
        }
    }
}

using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenChangingTheStartDateOfAnAgreementVersion
    {
        [TestCase(1, 2021, 6, 1, 1)]
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
            var newAgreementVersion = agreementVersion.ChangedStartDate(newStartDate);

            // Assert
            newAgreementVersion.MinimumRequiredVersion.Should().Be(minimumAgreementVersion);
        }
    }
}

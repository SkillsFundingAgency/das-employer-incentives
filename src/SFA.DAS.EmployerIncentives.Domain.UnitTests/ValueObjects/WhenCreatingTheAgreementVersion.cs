using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ValueObjects
{
    [TestFixture]
    public class WhenCreatingTheAgreementVersion
    {
        [TestCase(2020, 8, 1, 4)]
        [TestCase(2021, 1, 31, 4)]
        [TestCase(2021, 2, 1, 5)]
        [TestCase(2021, 3, 31, 5)]
        public void Then_the_minimum_agreement_version_is_set(int year, int month, int day, int minimumAgreementVersion)
        {
            // Arrange
            var plannedStartDate = new DateTime(year, month, day);
            // Act
            var agreementVersion = new AgreementVersion(plannedStartDate);

            // Assert
            agreementVersion.MinimumRequiredVersion.Should().Be(minimumAgreementVersion);
        }
    }
}

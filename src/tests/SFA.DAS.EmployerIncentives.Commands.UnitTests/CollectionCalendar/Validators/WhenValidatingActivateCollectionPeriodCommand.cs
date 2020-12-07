
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CollectionCalendar.Validators
{
    [TestFixture]
    public class WhenValidatingActivateCollectionPeriodCommand
    {
        private ActivateCollectionPeriodCommandValidator _sut;

        [SetUp]
        public void Arrange()
        {
            _sut = new ActivateCollectionPeriodCommandValidator();
        }

        [TestCase(0)]
        [TestCase(13)]
        public async Task Then_invalid_calendar_periods_result_in_validation_failure(byte collectionPeriod)
        {
            // Arrange
            var command = new ActivateCollectionPeriodCommand(collectionPeriod, 2020, true);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [Test]
        public async Task Then_invalid_calendar_year_results_in_validation_failure()
        {
            // Arrange
            var year = Convert.ToInt16(ValueObjects.NewApprenticeIncentive.EligibilityStartDate.Year - 1);
            var command = new ActivateCollectionPeriodCommand(1, year, true);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}

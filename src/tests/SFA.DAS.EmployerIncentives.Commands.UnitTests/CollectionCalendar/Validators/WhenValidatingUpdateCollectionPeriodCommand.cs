
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.CollectionCalendar;
using SFA.DAS.EmployerIncentives.Commands.CollectionPeriod;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.CollectionCalendar.Validators
{
    [TestFixture]
    public class WhenValidatingUpdateCollectionPeriodCommand
    {
        private UpdateCollectionPeriodCommandValidator _sut;

        [SetUp]
        public void Arrange()
        {
            _sut = new UpdateCollectionPeriodCommandValidator();
        }

        [TestCase(0)]
        [TestCase(13)]
        public async Task Then_invalid_calendar_periods_result_in_validation_failure(byte collectionPeriod)
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(collectionPeriod, "2021", true);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("202")]
        [TestCase("202A")]
        public async Task Then_invalid_academic_year_results_in_validation_failure(string academicYear)
        {
            // Arrange
            var command = new UpdateCollectionPeriodCommand(1, academicYear, true);

            // Act
            var result = await _sut.Validate(command);

            // Assert
            result.ValidationDictionary.Count.Should().Be(1);
        }
    }
}

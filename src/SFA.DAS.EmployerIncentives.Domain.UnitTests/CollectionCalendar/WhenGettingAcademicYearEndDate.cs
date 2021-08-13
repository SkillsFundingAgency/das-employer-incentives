using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.CollectionCalendarTests
{
    [TestFixture]
    public class WhenGettingAcademicYearEndDate
    {
        private CollectionCalendar _sut;
        private Fixture _fixture;
        
        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var academicYear1 = new AcademicYear("2021", new DateTime(2021, 07, 31));
            var academicYear2 = new AcademicYear("2122", new DateTime(2022, 07, 31));
            var academicYear3 = new AcademicYear("2223", new DateTime(2023, 07, 31));

            var academicYears = new List<AcademicYear> { academicYear1, academicYear2, academicYear3 };

            _sut = new CollectionCalendar(academicYears, new List<CollectionCalendarPeriod>());
        }

        [TestCase("2021", "2021-07-31")]
        [TestCase("2122", "2022-07-31")]
        [TestCase("2223", "2023-07-31")]
        public void Then_the_end_date_of_the_academic_year_is_returned(string academicYear, DateTime expectedEndDate)
        {
            var endDate = _sut.GetAcademicYearEndDate(academicYear);

            endDate.Should().Be(expectedEndDate);
        }

        [Test]
        public void Then_throws_custom_exception_when_unknown_year_supplied()
        {
            Action action = () => _sut.GetAcademicYearEndDate("1984");
            action.Should().Throw<AcademicYearNotFoundException>()
                .WithMessage("Unknown Academic Year: 1984");
        }
    }
}

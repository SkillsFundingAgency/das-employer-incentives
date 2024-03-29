﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.CollectionCalendarTests
{
    public class GetPeriod
    {
        private CollectionCalendar _sut;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Fixture _fixture;
        private DateTime testDate;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            testDate = DateTime.Now;

            var period1 = new CollectionCalendarPeriod(new CollectionPeriod(1, _fixture.Create<short>()), (byte)testDate.AddMonths(13).Month, (short)testDate.AddMonths(13).Year, _fixture.Create<DateTime>(), testDate.AddMonths(13), false, false);
            var period2 = new CollectionCalendarPeriod(new CollectionPeriod(2, _fixture.Create<short>()), (byte)testDate.Month, (short)testDate.Year, _fixture.Create<DateTime>(), testDate, false, false);
            var period3 = new CollectionCalendarPeriod(new CollectionPeriod(3, _fixture.Create<short>()), (byte)testDate.AddMonths(-13).Month, (short)testDate.AddMonths(-13).Year, _fixture.Create<DateTime>(), testDate.AddMonths(-13), false, false);

            _collectionPeriods = new List<CollectionCalendarPeriod>() { period1, period2, period3 };

            _sut = new CollectionCalendar(new List<AcademicYear>(), _collectionPeriods);
        }

        [Test]
        public void Then_the_latest_collection_period_is_returned_for_a_given_date()
        {
            // Arrange
            var date = testDate.AddDays(-1);

            // Act
            var period = _sut.GetPeriod(date);

            // Assert
            period.CollectionPeriod.PeriodNumber.Should().Be(2);
            period.CalendarMonth.Should().Be((byte)testDate.Month);
            period.CalendarYear.Should().Be((short)testDate.Year);
            period.CensusDate.Should().Be(testDate);
        }

        [Test]
        public void Then_null_is_returned_if_the_given_date_is_outside_the_collection_date_range()
        {
            // Arrange
            var date = testDate.AddMonths(13).AddDays(1);

            // Act
            var period = _sut.GetPeriod(date);

            // Assert
            period.Should().BeNull();
        }
    }
}

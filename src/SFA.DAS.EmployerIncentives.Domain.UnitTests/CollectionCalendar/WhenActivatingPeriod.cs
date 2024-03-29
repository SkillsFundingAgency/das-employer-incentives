﻿using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.CollectionCalendarTests
{
    [TestFixture]
    public class WhenActivatingPeriod
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

            var period1 = new CollectionCalendarPeriod(new CollectionPeriod(1, 2021), (byte)testDate.Month, (short)testDate.Year, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), true, false);
            var period2 = new CollectionCalendarPeriod(new CollectionPeriod(2, 2021), (byte)testDate.AddMonths(1).Month, (short)testDate.Year, testDate, _fixture.Create<DateTime>(), false, false);
            var period3 = new CollectionCalendarPeriod(new CollectionPeriod(3, 2021), (byte)testDate.AddMonths(2).Month, (short)testDate.Year, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, false);

            _collectionPeriods = new List<CollectionCalendarPeriod>() { period1, period2, period3 };

            _sut = new CollectionCalendar(new List<AcademicYear>(), _collectionPeriods);
        }

        [Test]
        public void Then_the_initial_period_is_set_to_active()
        {
            // Arrange / Act
            var activePeriod = _sut.GetPeriod(new CollectionPeriod(1, 2021));

            // Assert
            activePeriod.Active.Should().BeTrue();
        }

        [Test]
        public void Then_the_active_period_is_changed()
        {
            // Arrange / Act
            var period = new CollectionPeriod(2, 2021);
            _sut.SetActive(period);

            var periods = _sut.GetAllPeriods().ToList();

            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 1).Active.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 2).Active.Should().BeTrue();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 3).Active.Should().BeFalse();
            periods.Count(x => x.Active).Should().Be(1);
        }

        [Test]
        public void Then_the_period_in_progress_is_false_when_the_active_period_is_changed()
        {
            // Arrange / Act
            _collectionPeriods.Add(new CollectionCalendarPeriod(new CollectionPeriod(4, 2021), (byte)testDate.AddMonths(2).Month, (short)testDate.Year, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, true));
            var period = new CollectionPeriod(2, 2021);
            _sut.SetActive(period);

            var periods = _sut.GetAllPeriods().ToList();

            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 1).PeriodEndInProgress.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 2).PeriodEndInProgress.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 3).PeriodEndInProgress.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 4).PeriodEndInProgress.Should().BeFalse();
            periods.Count(x => x.Active).Should().Be(1);
        }

        [Test]
        public void Then_the_active_period_is_not_changed_when_the_period_and_year_not_matched()
        {
            // Arrange / Act            
            var period = new CollectionPeriod(4, 2021);
            _sut.SetActive(period);

            var periods = _sut.GetAllPeriods().ToList();

            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 1).Active.Should().BeTrue();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 2).Active.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 3).Active.Should().BeFalse();
            periods.Count(x => x.Active).Should().Be(1);
        }      
    }
}

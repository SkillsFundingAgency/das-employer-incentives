using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.CollectionCalendarTests
{
    [TestFixture]
    public class WhenSettingActivePeriodToInProgress
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

            var period1 = new CollectionCalendarPeriod(new CollectionPeriod(1, 2021),  (byte)testDate.Month, (short)testDate.Year, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, true);
            var period2 = new CollectionCalendarPeriod(new CollectionPeriod(2, 2021), (byte)testDate.AddMonths(1).Month, (short)testDate.Year, testDate, _fixture.Create<DateTime>(), true, false);
            var period3 = new CollectionCalendarPeriod(new CollectionPeriod(3, 2021), (byte)testDate.AddMonths(2).Month, (short)testDate.Year, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), false, false);

            _collectionPeriods = new List<CollectionCalendarPeriod>() { period1, period2, period3 };

            _sut = new CollectionCalendar(_collectionPeriods);
        }

        [Test]
        public void Then_the_active_period_is_set_to_in_progress()
        {
            // Arrange / Act
            _sut.SetActivePeriodToInProgress();

            var periods = _sut.GetAllPeriods().ToList();

            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 1).PeriodEndInProgress.Should().BeFalse();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 2).PeriodEndInProgress.Should().BeTrue();
            periods.FirstOrDefault(x => x.CollectionPeriod.PeriodNumber == 3).PeriodEndInProgress.Should().BeFalse();
            periods.Count(x => x.PeriodEndInProgress == true).Should().Be(1);
        }
    }
}

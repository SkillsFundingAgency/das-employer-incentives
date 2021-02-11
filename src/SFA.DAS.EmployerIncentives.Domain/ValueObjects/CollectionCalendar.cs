using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class CollectionCalendar : ValueObject
    {
        private readonly IEnumerable<CollectionPeriod> _collectionPeriods;

        public CollectionCalendar(IEnumerable<CollectionPeriod> collectionPeriods)
        {
            _collectionPeriods = collectionPeriods;
        }

        public CollectionPeriod GetPeriod(DateTime dateTime)
        {
            var period =
                _collectionPeriods
                .Where(d => d.OpenDate <= dateTime)
                .OrderByDescending(d => d.OpenDate)
                .FirstOrDefault();

            return period;
        }

        public CollectionPeriod GetActivePeriod()
        {
            return
                _collectionPeriods
                .Single(d => d.Active);
        }

        public CollectionPeriod GetNextPeriod(CollectionPeriod period)
        {
            var nextPeriodDate = new DateTime(period.CalendarYear, period.CalendarMonth, 1).AddMonths(1);
            return
                _collectionPeriods
                .Single(d => d.CalendarMonth == nextPeriodDate.Month && d.CalendarYear == nextPeriodDate.Year);
        }

        public CollectionPeriod GetPeriod(short collectionYear, byte periodNumber)
        {
            return 
                _collectionPeriods
                .Single(d => d.AcademicYear == collectionYear && d.PeriodNumber == periodNumber);
        }

        public void SetActive(CollectionPeriod collectionPeriod)
        {
            var collectionPeriodToActivate = _collectionPeriods.FirstOrDefault(x => x.AcademicYear == collectionPeriod.AcademicYear 
                                                                                 && x.PeriodNumber == collectionPeriod.PeriodNumber);
            if (collectionPeriodToActivate == null)
            {
                return;
            }

            foreach (var collectionCalendarPeriod in _collectionPeriods)
            {
                collectionCalendarPeriod.SetActive(false);
            }           

            collectionPeriodToActivate.SetActive(true);
        }

        public ReadOnlyCollection<CollectionPeriod> GetAllPeriods()
        {
            return new ReadOnlyCollection<CollectionPeriod>(_collectionPeriods.ToList());
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            foreach (var collectionPeriod in _collectionPeriods)
            {
                yield return collectionPeriod.PeriodNumber;
                yield return collectionPeriod.CalendarMonth;
                yield return collectionPeriod.CalendarYear;
                yield return collectionPeriod.AcademicYear;
                yield return collectionPeriod.OpenDate;
            }
        }
    }
}
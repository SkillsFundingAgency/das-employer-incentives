using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class CollectionCalendar : ValueObject
    {
        private readonly IEnumerable<AcademicYear> _academicYears;
        private readonly IEnumerable<CollectionCalendarPeriod> _collectionPeriods;

        public CollectionCalendar(IEnumerable<AcademicYear> academicYears, IEnumerable<CollectionCalendarPeriod> collectionPeriods)
        {
            _academicYears = academicYears;
            _collectionPeriods = collectionPeriods;
        }

        public CollectionCalendarPeriod GetPeriod(DateTime dateTime)
        {
            return _collectionPeriods
                .Where(d => d.OpenDate <= dateTime)
                .OrderByDescending(d => d.OpenDate)
                .FirstOrDefault();
        }

        public CollectionCalendarPeriod GetPeriod(CollectionPeriod collectionPeriod)
        {
            return collectionPeriod == null
                ? null
                : _collectionPeriods
                .Single(d => d.CollectionPeriod == collectionPeriod);
        }

        public CollectionCalendarPeriod GetActivePeriod()
        {
            return
                _collectionPeriods
                .Single(d => d.Active);
        }        

        public CollectionCalendarPeriod GetNextPeriod(CollectionCalendarPeriod period)
        {
            var nextPeriodDate = new DateTime(period.CalendarYear, period.CalendarMonth, 1).AddMonths(1);
            return
                _collectionPeriods
                .Single(d => d.CalendarMonth == nextPeriodDate.Month && d.CalendarYear == nextPeriodDate.Year);
        }        

        public void SetActive(CollectionPeriod collectionPeriod)
        {
            var collectionPeriodToActivate = _collectionPeriods.FirstOrDefault(x => x.CollectionPeriod == collectionPeriod);

            if (collectionPeriodToActivate == null)
            {
                return;
            }

            foreach (var collectionCalendarPeriod in _collectionPeriods)
            {
                collectionCalendarPeriod.SetActive(false);
                collectionCalendarPeriod.SetPeriodEndInProgress(false);
            }           

            collectionPeriodToActivate.SetActive(true);
        }

        public void SetActivePeriodToInProgress()
        {
            foreach (var collectionCalendarPeriod in _collectionPeriods)
            {
                collectionCalendarPeriod.SetPeriodEndInProgress(false);
            }

            var activePeriod = GetActivePeriod();
            activePeriod.SetPeriodEndInProgress(true);
        }

        public ReadOnlyCollection<CollectionCalendarPeriod> GetAllPeriods()
        {
            return new ReadOnlyCollection<CollectionCalendarPeriod>(_collectionPeriods.ToList());
        }

        public DateTime GetAcademicYearEndDate(string academicYearId)
        {
            if (_academicYears.All(ay => ay.AcademicYearId != academicYearId))
            {
                throw new AcademicYearNotFoundException($"Unknown Academic Year: {academicYearId}");
            }
            var academicYear = _academicYears.Single(x => x.AcademicYearId == academicYearId);
            return academicYear.EndDate;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            foreach (var collectionPeriod in _collectionPeriods)
            {
                yield return collectionPeriod.CollectionPeriod;
                yield return collectionPeriod.CalendarMonth;
                yield return collectionPeriod.CalendarYear;
                yield return collectionPeriod.OpenDate;
            }
        }
    }
}
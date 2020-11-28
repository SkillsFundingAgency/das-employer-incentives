﻿using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;
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

        public CollectionPeriod GetPeriod(short collectionYear, byte periodNumber)
        {
            return
                _collectionPeriods
                .Single(d => d.CalendarYear == collectionYear && d.PeriodNumber == periodNumber);
        }

        public void ActivatePeriod(short collectionYear, byte periodNumber)
        {
            var collectionPeriodToActivate = _collectionPeriods.FirstOrDefault(x => x.CalendarYear == collectionYear && x.PeriodNumber == periodNumber);

            if (collectionPeriodToActivate == null)
            {
                return;
            }

            foreach (var collectionPeriod in _collectionPeriods)
            {
                collectionPeriod.SetActive(false);
            }

            collectionPeriodToActivate.SetActive(true);
        }

        public IEnumerable<CollectionPeriod> GetAllPeriods()
        {
            return _collectionPeriods;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            foreach (var collectionPeriod in _collectionPeriods)
            {
                yield return collectionPeriod.PeriodNumber;
                yield return collectionPeriod.CalendarMonth;
                yield return collectionPeriod.CalendarYear;
                yield return collectionPeriod.OpenDate;
            }
        }
    }
}
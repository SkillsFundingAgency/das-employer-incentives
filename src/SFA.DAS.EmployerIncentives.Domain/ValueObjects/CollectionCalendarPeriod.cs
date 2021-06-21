using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{   
    public class CollectionCalendarPeriod : ValueObject
    {
        public CollectionCalendarPeriod(CollectionPeriod collectionPeriod, byte calendarMonth, short calendarYear, DateTime openDate, DateTime censusDate, bool active) 
        {
            CollectionPeriod = collectionPeriod;
            CalendarMonth = calendarMonth;
            CalendarYear = calendarYear;
            OpenDate = openDate;
            CensusDate = censusDate;
            Active = active;
        }

        public CollectionPeriod CollectionPeriod { get; }
        public byte CalendarMonth { get; }
        public short CalendarYear { get; }
        public DateTime OpenDate { get; }
        public DateTime CensusDate { get; }
        public bool Active { get; private set; }

        public void SetActive(bool active)
        {
            Active = active;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CollectionPeriod;
            yield return CalendarMonth;
            yield return CalendarYear;
            yield return OpenDate;
            yield return CensusDate;
            yield return Active;
        }
    }
}
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{   
    public class CollectionPeriod : ValueObject
    {
        public CollectionPeriod(byte periodNumber, byte calendarMonth, short calendarYear, DateTime openDate, DateTime censusDate, short academicYear, bool active) 
        {
            PeriodNumber = periodNumber;
            AcademicYear = academicYear;
            CalendarMonth = calendarMonth;
            CalendarYear = calendarYear;
            OpenDate = openDate;
            CensusDate = censusDate;
            Active = active;
        }

        public byte PeriodNumber { get; }
        public short AcademicYear { get; }
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
            yield return PeriodNumber;
            yield return CalendarMonth;
            yield return CalendarYear;
            yield return OpenDate;
            yield return CensusDate;
            yield return AcademicYear;
            yield return Active;
        }
    }
}
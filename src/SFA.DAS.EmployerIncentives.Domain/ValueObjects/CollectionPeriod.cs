using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class CollectionPeriod : ValueObject
    {
        public CollectionPeriod(byte periodNumber, byte calendarMonth, short calendarYear, DateTime openDate, DateTime censusDate, short academicYear, bool active)
        {
            PeriodNumber = periodNumber;
            CalendarMonth = calendarMonth;
            CalendarYear = calendarYear;
            OpenDate = openDate;
            CensusDate = censusDate;
            AcademicYear = academicYear;
            Active = active;
        }

        public byte PeriodNumber { get; }
        public byte CalendarMonth { get; }
        public short CalendarYear { get; }
        public DateTime OpenDate { get; }
        public DateTime CensusDate { get; }
        public short AcademicYear { get; }
        public bool Active { get; }

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
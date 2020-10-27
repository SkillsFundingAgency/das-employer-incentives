using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class CollectionPeriod : ValueObject
    {
        public CollectionPeriod(byte periodNumber, byte calendarMonth, short calendarYear, DateTime openDate)
        {
            PeriodNumber = periodNumber;
            CalendarMonth = calendarMonth;
            CalendarYear = calendarYear;
            OpenDate = openDate;
        }

        public byte PeriodNumber { get; }
        public byte CalendarMonth { get; }
        public short CalendarYear { get; }
        public DateTime OpenDate { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return PeriodNumber;
            yield return CalendarMonth;
            yield return CalendarYear;
            yield return OpenDate;
        }
    }
}
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class NextPendingPayment : ValueObject
    {
        public short CollectionYear { get; }
        public byte CollectionPeriod { get; }
        public DateTime DueDate { get; }

        public NextPendingPayment(short year, byte period, DateTime due)
        {
            CollectionYear = year;
            CollectionPeriod = period;
            DueDate = due;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CollectionYear;
            yield return CollectionPeriod;
            yield return DueDate;
        }
    }
}

using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class BreakInLearning : ValueObject
    {
        public BreakInLearning(DateTime startDate)
        {
            StartDate = startDate.Date;
        }

        public DateTime StartDate { get; }
        public DateTime? EndDate { get; private set; }
        public int Days => EndDate.HasValue ? (EndDate.Value - StartDate).Days : 0;

        public BreakInLearning SetEndDate(DateTime dateTime)
        {
            EndDate = dateTime.Date;
            return this;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}

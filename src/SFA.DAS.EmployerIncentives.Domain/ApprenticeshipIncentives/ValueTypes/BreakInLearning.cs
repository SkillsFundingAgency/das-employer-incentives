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

        public static BreakInLearning Create(DateTime startDate, DateTime endDate)
        {
            var breakInLearning = new BreakInLearning(startDate);
            breakInLearning.SetEndDate(endDate);
            return breakInLearning;
        }

        public DateTime StartDate { get; }
        public DateTime? EndDate { get; private set; }
        public int Days => EndDate.HasValue ? (EndDate.Value - StartDate).Days : 0;

        private void SetEndDate(DateTime value)
        {
            if (value.Date < StartDate)
            {
                throw new ArgumentException("End date of break in learning can't be before the start date");
            }
            EndDate = value.Date;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}

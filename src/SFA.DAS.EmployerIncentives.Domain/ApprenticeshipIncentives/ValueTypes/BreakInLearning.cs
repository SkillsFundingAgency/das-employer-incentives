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

        public void SetEndDate(DateTime dateTime)
        {
            if (dateTime.Date < StartDate)
            {
                throw new ArgumentException("End date of break in learning can't be before the start date");
            }
            EndDate = dateTime.Date;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}

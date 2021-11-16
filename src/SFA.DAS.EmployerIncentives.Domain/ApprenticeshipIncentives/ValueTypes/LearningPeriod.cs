using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class LearningPeriod : ValueObject
    {
        public LearningPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("End date of learning period can't be before the start date");
            }

            else
            {
                StartDate = startDate;
                EndDate = endDate;
            }
        }

        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return StartDate;
            yield return EndDate;
        }
    }
}
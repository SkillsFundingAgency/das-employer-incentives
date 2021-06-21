using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class DaysInLearning : ValueObject
    {
        public DaysInLearning(CollectionPeriod collectionPeriod, int numberOfDays)
        {
            CollectionPeriod = collectionPeriod;
            NumberOfDays = numberOfDays;
        }

        public CollectionPeriod CollectionPeriod { get; }
        public int NumberOfDays { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CollectionPeriod;
            yield return NumberOfDays;
        }
    }
}

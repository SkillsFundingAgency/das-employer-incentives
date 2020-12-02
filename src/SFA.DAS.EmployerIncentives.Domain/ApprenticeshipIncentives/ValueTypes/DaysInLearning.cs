using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class DaysInLearning : ValueObject
    {
        public DaysInLearning(byte collectionPeriodNumber, short collectionYear, int numberOfDays)
        {
            CollectionPeriodNumber = collectionPeriodNumber;
            CollectionYear = collectionYear;
            NumberOfDays = numberOfDays;
        }

        public byte CollectionPeriodNumber { get; }
        public short CollectionYear { get; }
        public int NumberOfDays { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return CollectionPeriodNumber;
            yield return CollectionYear;
            yield return NumberOfDays;
        }
    }
}

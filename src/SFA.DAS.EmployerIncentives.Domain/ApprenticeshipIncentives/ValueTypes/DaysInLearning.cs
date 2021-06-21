using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class DaysInLearning : ValueObject
    {
        public DaysInLearning(AcademicPeriod academicPeriod, int numberOfDays)
        {
            AcademicPeriod = academicPeriod;
            NumberOfDays = numberOfDays;
        }

        public AcademicPeriod AcademicPeriod { get; }
        public int NumberOfDays { get; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return AcademicPeriod;
            yield return NumberOfDays;
        }
    }
}

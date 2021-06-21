using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{
    public class AcademicPeriod : ValueObject
    {
        public byte PeriodNumber { get; }
        public short AcademicYear { get; }

        public AcademicPeriod(byte periodNumber, short academicYear)
        {
            PeriodNumber = periodNumber;
            AcademicYear = academicYear;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return PeriodNumber;
            yield return AcademicYear;
        }
    }
}
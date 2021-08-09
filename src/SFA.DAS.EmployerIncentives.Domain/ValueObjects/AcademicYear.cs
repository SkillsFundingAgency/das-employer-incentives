using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ValueObjects
{   
    public class AcademicYear : ValueObject
    {
        public string AcademicYearId { get; }
        public DateTime EndDate { get; }

        public AcademicYear(string academicYear, DateTime endDate)
        {
            AcademicYearId = academicYear;
            EndDate = endDate;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return AcademicYearId;
            yield return EndDate;
        }
    }
}
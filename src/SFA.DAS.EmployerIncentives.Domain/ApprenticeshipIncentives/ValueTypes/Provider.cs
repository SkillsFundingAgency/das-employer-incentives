using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Provider : ValueObject
    {
        public long Ukprn { get; }

        public Provider(long ukprn)
        {
            if (ukprn <= 0) throw new ArgumentException("ukprn must be greater than 0", nameof(ukprn));

            Ukprn = ukprn;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Ukprn;
        }
    }
}

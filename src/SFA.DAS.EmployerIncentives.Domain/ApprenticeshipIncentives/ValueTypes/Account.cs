using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Account : ValueObject
    {
        public long Id { get; }

        public Account(long id)
        {
            if(id <= 0) throw new ArgumentException("Account Id must be greater than 0", nameof(id));

            Id = id;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
        }
    }
}

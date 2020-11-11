using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes
{
    public class Account : ValueObject
    {
        public long Id { get; }
        public long AccountLegalEntityId { get; }

        public Account(long id, long accountLegalEntityId)
        {
            if (id <= 0) throw new ArgumentException("Account Id must be greater than 0", nameof(id));
            if (accountLegalEntityId <= 0) throw new ArgumentException("Account LegalEntity Id must be greater than 0", nameof(accountLegalEntityId));

            Id = id;
            AccountLegalEntityId = accountLegalEntityId;
        }
      
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Id;
            yield return AccountLegalEntityId;
        }
    }
}

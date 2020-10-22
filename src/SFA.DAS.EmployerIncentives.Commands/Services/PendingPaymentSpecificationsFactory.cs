using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Specifications;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public class PendingPaymentSpecificationsFactory : ISpecificationsFactory<PendingPayment>
    {
        private readonly List<Specification<PendingPayment>> _rules;

        public IEnumerable<Specification<PendingPayment>> Rules => _rules;

        public PendingPaymentSpecificationsFactory(IAccountDomainRepository accountDomainRepository)
        {
            _rules = new List<Specification<PendingPayment>>
            {
                new PendingPaymentHasBankDetails(accountDomainRepository)
            };
        }      
    }
}

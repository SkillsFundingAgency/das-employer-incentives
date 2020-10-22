using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.Specifications
{
    public class PendingPaymentHasBankDetails : Specification<PendingPayment>
    {
        public override string Name => "HasBankDetails";

        private readonly IAccountDomainRepository _accountDomainRepository;

        public PendingPaymentHasBankDetails(IAccountDomainRepository accountDomainRepository)
        {
            _accountDomainRepository = accountDomainRepository;
        }

        public override Expression<Func<PendingPayment, bool>> ToExpression()
        {
            return pendingPayment => IsValid(pendingPayment);
        }

        private bool IsValid(PendingPayment pendingPayment)
        {
            Domain.Accounts.Account account = Data as Domain.Accounts.Account;
            var legalEntity = account.GetLegalEntity(pendingPayment.Account.AccountLegalEntityId);
            return !string.IsNullOrEmpty(legalEntity.VrfVendorId);
        }

        public async override Task<dynamic> Fetch(PendingPayment entity)
        {
            var account = await _accountDomainRepository.Find(entity.Account.Id);
            return account;
        }
    }
}

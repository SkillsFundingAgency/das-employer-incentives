using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{
    public class SignLegalEntityAgreementCommandHandler : ICommandHandler<SignLegalEntityAgreementCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public SignLegalEntityAgreementCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(SignLegalEntityAgreementCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
            var account = await _domainRepository.Find(command.AccountId);
            if (account != null) 
            {
                if (account.GetLegalEntity(command.AccountLegalEntityId) != null)
                {
                    return; // already created
                }
            }
            else
            {
                account = Account.New(command.AccountId);
            }

            account.AddLegalEntity(command.AccountLegalEntityId, LegalEntity.New(command.LegalEntityId, command.Name));

            await _domainRepository.Save(account);
        }
    }
}

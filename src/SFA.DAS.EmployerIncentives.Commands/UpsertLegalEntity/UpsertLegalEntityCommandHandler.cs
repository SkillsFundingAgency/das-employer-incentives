using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Accounts;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerIncentives.Commands.UpsertLegalEntity
{
    public class UpsertLegalEntityCommandHandler : ICommandHandler<UpsertLegalEntityCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;
        private readonly IEncodingService _encodingService;

        public UpsertLegalEntityCommandHandler(IAccountDomainRepository domainRepository, IEncodingService encodingService)
        {
            _domainRepository = domainRepository;
            _encodingService = encodingService;
        }

        public async Task Handle(UpsertLegalEntityCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _domainRepository.Find(command.AccountId);
            if (account == null)
            {
                account = Account.New(command.AccountId);
            }

            var legalEntity = account.GetLegalEntity(command.AccountLegalEntityId);
            if (account.GetLegalEntity(command.AccountLegalEntityId) != null)
            {
                legalEntity.SetHashedLegalEntityId(_encodingService.Encode(command.LegalEntityId, EncodingType.AccountId));
                legalEntity.SetLegalEntityName(command.Name);
            }
            else
            {
                account.AddLegalEntity(command.AccountLegalEntityId, LegalEntity.New(command.LegalEntityId, command.Name,_encodingService.Encode(command.LegalEntityId, EncodingType.AccountId)));
            }

            await _domainRepository.Save(account);
        }
    }
}

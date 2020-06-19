using SFA.DAS.EmployerIncentives.Application.Exceptions;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommandHandler : ICommandHandler<AddLegalEntityCommand>
    {
        private readonly IValidator<AddLegalEntityCommand> _validator;
        private readonly IDomainRepository<long, Account> _domainRepository;

        public AddLegalEntityCommandHandler(
            IValidator<AddLegalEntityCommand> validator,
            IDomainRepository<long, Account> domainRepository)
        {
            _validator = validator;
            _domainRepository = domainRepository;
        }

        public async Task Handle(AddLegalEntityCommand command)
        {
            var validationResult = await _validator.Validate(command);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var account = await _domainRepository.Find(command.AccountId);
            if (account != null) 
            {
                if (account.LegalEntities.Any(l => l.AccountLegalEntityId == command.AccountLegalEntityId))
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

using SFA.DAS.EmployerIncentives.Application.Exceptions;
using SFA.DAS.EmployerIncentives.Application.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Entities;
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

            if (await _domainRepository.Find(command.AccountId) != null)
            {
                return; // already created
            }

            var account = Account.New(command.AccountId);            
            var legalEntity = LegalEntity.New(command.LegalEntityId, command.Name);
            account.AddLegalEntity(command.AccountLegalEntityId, legalEntity);

            await _domainRepository.Save(account);
        }
    }
}

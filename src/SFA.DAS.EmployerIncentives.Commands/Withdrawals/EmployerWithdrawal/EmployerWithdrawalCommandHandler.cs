using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.EmployerWithdrawal
{
    public class EmployerWithdrawalCommandHandler : ICommandHandler<EmployerWithdrawalCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IAccountDomainRepository _accountDomainRepository;

        public EmployerWithdrawalCommandHandler(IIncentiveApplicationDomainRepository domainRepository,
            IAccountDomainRepository accountDomainRepository)
        {
            _domainRepository = domainRepository;
            _accountDomainRepository = accountDomainRepository;
        }

        public async Task Handle(EmployerWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var applications = (await _domainRepository.Find(command.AccountLegalEntityId, command.ULN)).ToList();
            if(!applications.Any())
            {
                throw new WithdrawalException($"Unable to handle Employer withdrawal command. No matching incentive applications found for {command}");
            }

            foreach (var application in applications)
            {
                var account = await _accountDomainRepository.Find(application.AccountId);
                var legalEntity = account.GetLegalEntity(application.AccountLegalEntityId);

                foreach (var apprenticeship in application.Apprenticeships)
                {
                    if(apprenticeship.ULN == command.ULN)
                    {
                        application.EmployerWithdrawal(
                            apprenticeship,
                            legalEntity,
                            command.EmailAddress,
                            new ServiceRequest(
                                command.ServiceRequestTaskId, 
                                command.DecisionReference,
                                command.ServiceRequestCreated));
                    }
                }

                await _domainRepository.Save(application);
            }
        }
    }
}

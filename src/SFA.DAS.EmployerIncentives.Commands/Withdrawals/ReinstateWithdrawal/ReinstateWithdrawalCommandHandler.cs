using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ReinstateWithdrawalCommandHandler : ICommandHandler<ReinstateWithdrawalCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public ReinstateWithdrawalCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(ReinstateWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _domainRepository.Find(command.AccountLegalEntityId, command.ULN);
            if(!applications.Any())
            {
                throw new WithdrawalException($"Unable to handle reinstate withdrawal command.  No matching incentive applications found for {command}");
            }

            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    if(apprenticeship.ULN == command.ULN && apprenticeship.WithdrawnByCompliance)
                    {
                        application.ReinstateWithdrawal(apprenticeship);
                    }
                }
                await _domainRepository.Save(application);
            }
        }
    }
}

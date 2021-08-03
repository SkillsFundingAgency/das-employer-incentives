using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ComplianceWithdrawalCommandHandler : ICommandHandler<ComplianceWithdrawalCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public ComplianceWithdrawalCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(ComplianceWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _domainRepository.Find(command);
            if(!applications.Any())
            {
                throw new WithdrawalException($"Unable to handle Compliance withdrawal command.  No matching incentive applications found for {command}");
            }

            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    if(apprenticeship.ULN == command.ULN)
                    {
                        application.ComplianceWithdrawal(
                            apprenticeship, 
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

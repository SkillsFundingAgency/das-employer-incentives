using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
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

        public EmployerWithdrawalCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(EmployerWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _domainRepository.Find(command);
            if(!applications.Any())
            {
                throw new EmployerWithdrawalException($"Unable to handle Employer withdrawal command.  No matching incentive applications found for {command}");
            }

            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    if(apprenticeship.ULN == command.ULN)
                    {
                        application.EmployerWithdrawal(
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

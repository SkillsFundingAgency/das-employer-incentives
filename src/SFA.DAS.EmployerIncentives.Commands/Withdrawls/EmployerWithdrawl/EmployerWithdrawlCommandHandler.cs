using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawls.EmployerWithdrawl
{
    public class EmployerWithdrawlCommandHandler : ICommandHandler<EmployerWithdrawlCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public EmployerWithdrawlCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(EmployerWithdrawlCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _domainRepository.Find(command);
            if(!applications.Any())
            {
                throw new EmployerWithdrawlException($"Unable to handle Employer withdrawl command.  No matching incentive applications found for {command}");
            }
            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    if(apprenticeship.ULN == command.ULN)
                    {
                        application.EmployerWithdrawn(
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

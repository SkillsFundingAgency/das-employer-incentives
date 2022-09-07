using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ComplianceWithdrawal
{
    public class ReinstateWithdrawalCommandHandler : ICommandHandler<ReinstateWithdrawalCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IIncentiveApplicationStatusAuditDataRepository _incentiveApplicationStatusAuditDataRepository;

        public ReinstateWithdrawalCommandHandler(IIncentiveApplicationDomainRepository domainRepository,
            IIncentiveApplicationStatusAuditDataRepository incentiveApplicationStatusAuditDataRepository)
        {
            _domainRepository = domainRepository;
            _incentiveApplicationStatusAuditDataRepository = incentiveApplicationStatusAuditDataRepository; 
        }

        public async Task Handle(ReinstateWithdrawalCommand command, CancellationToken cancellationToken = default)
        {
            var applications = await _domainRepository.Find(command.AccountLegalEntityId, command.ULN);
            if(!applications.Any())
            {
                throw new WithdrawalException($"Unable to handle reinstate withdrawal command.  No matching incentive applications found for {command}");
            }

            var applicationToReinstate = applications.Where(x => x.Status == IncentiveApplicationStatus.ComplianceWithdrawn)
                                                     .OrderByDescending(x => x.DateSubmitted).FirstOrDefault();
            
            if (applicationToReinstate == null)
            {
                throw new WithdrawalException($"Unable to handle reinstate withdrawal command.  No matching incentive applications found for {command}");
            }

            var apprenticeship = applicationToReinstate.Apprenticeships.FirstOrDefault(x => x.ULN == command.ULN);
            
            applicationToReinstate.ReinstateWithdrawal(apprenticeship,
                new ServiceRequest(
                    command.ServiceRequestTaskId,
                    command.DecisionReference,
                    command.ServiceRequestCreated));
    
            await _domainRepository.Save(applicationToReinstate);
        }
    }
}

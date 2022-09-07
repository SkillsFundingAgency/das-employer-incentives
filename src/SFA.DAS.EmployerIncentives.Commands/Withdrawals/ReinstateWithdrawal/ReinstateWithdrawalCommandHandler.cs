using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.Withdrawals;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.Withdrawals.ReinstateWithdrawal
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
            if(applications == null || !applications.Any())
            {
                throw new WithdrawalException($"Unable to handle reinstate withdrawal command.  No matching incentive applications found for ULN {command.ULN}");
            }

            var submittedApprenticeships = new List<SubmittedApprenticeship>();
            foreach(var application in applications)
            {
                foreach(var apprenticeship in application.Apprenticeships)
                {
                    if (apprenticeship.ULN == command.ULN && apprenticeship.WithdrawnByCompliance)
                    {
                        submittedApprenticeships.Add(new SubmittedApprenticeship(application.Id, apprenticeship.Id, application.DateSubmitted));
                    }
                }
            }

            var submittedApprenticeshipToReinstate = submittedApprenticeships.OrderByDescending(x => x.DateSubmitted).FirstOrDefault();
            if (submittedApprenticeshipToReinstate == null)
            {
                throw new WithdrawalException($"Unable to handle reinstate withdrawal command.  No matching incentive applications found for ULN {command.ULN}");
            }

            var applicationToReinstate = applications.Single(x => x.Id == submittedApprenticeshipToReinstate.IncentiveApplicationId);

            var apprenticeshipToReinstate = applicationToReinstate.Apprenticeships.FirstOrDefault(x => x.ULN == command.ULN);

            applicationToReinstate.ReinstateWithdrawal(apprenticeshipToReinstate,
                new ServiceRequest(
                    command.ServiceRequestTaskId,
                    command.DecisionReference,
                    command.ServiceRequestCreated));

            await _domainRepository.Save(applicationToReinstate);
        }
    }
}

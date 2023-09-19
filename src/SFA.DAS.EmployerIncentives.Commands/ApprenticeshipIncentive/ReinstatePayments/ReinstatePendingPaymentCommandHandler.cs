using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ReinstatePayments
{
    public class ReinstatePendingPaymentCommandHandler : ICommandHandler<ReinstatePendingPaymentCommand>
    {
        private readonly IApprenticeshipIncentiveArchiveRepository _apprenticeshipIncentiveArchiveRepository;
        private readonly IApprenticeshipIncentiveDomainRepository _apprenticeshipIncentiveDomainRepository;

        public ReinstatePendingPaymentCommandHandler(IApprenticeshipIncentiveArchiveRepository apprenticeshipIncentiveArchiveRepository,
                                                     IApprenticeshipIncentiveDomainRepository apprenticeshipIncentiveDomainRepository)
        {
            _apprenticeshipIncentiveArchiveRepository = apprenticeshipIncentiveArchiveRepository;
            _apprenticeshipIncentiveDomainRepository = apprenticeshipIncentiveDomainRepository;
        }

        public async Task Handle(ReinstatePendingPaymentCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            var pendingPayment = await _apprenticeshipIncentiveArchiveRepository.GetArchivedPendingPayment(command.PendingPaymentId);

            if (pendingPayment == null)
            {
                throw new ArgumentException($"Pending payment with ID {command.PendingPaymentId} not found");
            }

            var incentive = await _apprenticeshipIncentiveDomainRepository.Find(pendingPayment.ApprenticeshipIncentiveId);

            if (incentive == null)
            {
                throw new ArgumentException($"Apprenticeship incentive with ID {pendingPayment.ApprenticeshipIncentiveId} for pending payment ID {pendingPayment.Id} not found");
            }

            if (incentive.PendingPayments.Any(x => x.Id == pendingPayment.Id))
            {
                throw new ArgumentException($"Apprenticeship incentive with ID {pendingPayment.ApprenticeshipIncentiveId} pending payment ID {command.PendingPaymentId} already exists");
            }

            incentive.ReinstatePendingPayment(pendingPayment, command.ReinstatePaymentRequest);

            await _apprenticeshipIncentiveDomainRepository.Save(incentive);
        }
    }
}

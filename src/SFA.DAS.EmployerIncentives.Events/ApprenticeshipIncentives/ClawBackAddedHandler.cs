using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class ClawBackAddedHandler : IDomainEventHandler<ClawBackAdded>
    {
        private readonly IClawbackDataRepository _clawbackRepository;

        public ClawBackAddedHandler(IClawbackDataRepository clawbackRepository)
        {
            _clawbackRepository = clawbackRepository;
        }

        public async Task Handle(ClawBackAdded @event, CancellationToken cancellationToken = default)
        {
            var payments = @event.Model.PaymentModels.ToList();
            
            foreach(var pendingPayment in @event.Model.PendingPaymentModels)
            {
                if(pendingPayment.ClawedBack)
                {
                    var payment = payments.Single(p => p.PendingPaymentId == pendingPayment.Id);

                    await _clawbackRepository.Add(new Domain.ApprenticeshipIncentives.ValueTypes.Clawback(
                        Guid.NewGuid(),
                        pendingPayment.ApprenticeshipIncentiveId,
                        pendingPayment.Id,
                        pendingPayment.Account,
                        pendingPayment.Amount,
                        DateTime.UtcNow,
                        payment.SubnominalCode,
                        payment.Id));
                }
            }
        }
    }
}

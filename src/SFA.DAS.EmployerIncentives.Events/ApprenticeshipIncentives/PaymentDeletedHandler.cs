using SFA.DAS.EmployerIncentives.Abstractions.Events;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives
{
    public class PaymentDeletedHandler : IDomainEventHandler<PaymentDeleted>
    {
        private readonly IApprenticeshipIncentiveArchiveRepository _archiveRepository;

        public PaymentDeletedHandler(IApprenticeshipIncentiveArchiveRepository archiveRepository)
        {
            _archiveRepository = archiveRepository;
        }

        public Task Handle(PaymentDeleted @event, CancellationToken cancellationToken = default)
        {
            return _archiveRepository.Archive(@event.Model);            
        }
    }
}

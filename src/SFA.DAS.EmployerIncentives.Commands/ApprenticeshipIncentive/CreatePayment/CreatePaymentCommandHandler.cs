using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.CreatePayment
{
    public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand>
    {
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;
        
        public CreatePaymentCommandHandler(IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(CreatePaymentCommand command, CancellationToken cancellationToken = default)
        {
            var incentive = await _domainRepository.Find(command.ApprenticeshipIncentiveId);

            incentive.CreatePayment(command.PendingPaymentId, new Domain.ValueObjects.CollectionPeriod(command.CollectionPeriod, command.CollectionYear));

            await _domainRepository.Save(incentive);
        }
    }
}

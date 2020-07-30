using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class SubmitIncentiveApplicationCommandHandler : ICommandHandler<SubmitIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public SubmitIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(SubmitIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            application.Submit(command.DateSubmitted, command.SubmittedBy);

            await _domainRepository.Save(application);
        }
    }
}

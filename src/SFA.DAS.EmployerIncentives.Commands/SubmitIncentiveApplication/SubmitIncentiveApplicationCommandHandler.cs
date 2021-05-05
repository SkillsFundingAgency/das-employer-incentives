using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class SubmitIncentiveApplicationCommandHandler : ICommandHandler<SubmitIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;
        private readonly IUlnValidationService _ulnValidationService;

        public SubmitIncentiveApplicationCommandHandler(IIncentiveApplicationDomainRepository domainRepository, IUlnValidationService ulnValidationService)
        {
            _domainRepository = domainRepository;
            _ulnValidationService = ulnValidationService;
        }

        public async Task Handle(SubmitIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }

            foreach (var apprenticeship in application.Apprenticeships)
            {
                if (await _ulnValidationService.UlnAlreadyOnSubmittedIncentiveApplication(apprenticeship.ULN))
                {
                    //TODO: New exception type?
                    throw new InvalidRequestException();
                }
            }

            application.Submit(command.DateSubmitted, command.SubmittedByEmail, command.SubmittedByName);

            await _domainRepository.Save(application);
        }
    }
}

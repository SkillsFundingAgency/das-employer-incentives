using System;
using System.Linq;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.SubmitIncentiveApplication;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class SubmitIncentiveApplicationCommandHandler : ICommandHandler<SubmitIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationDomainRepository _domainRepository;        

        public SubmitIncentiveApplicationCommandHandler(IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(SubmitIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            if (application == null || application.AccountId != command.AccountId)
            {
                throw new InvalidRequestException();
            }
            if (application.Apprenticeships.Any(apprenticeship => !apprenticeship.EmploymentStartDate.HasValue || apprenticeship.EmploymentStartDate.Value == DateTime.MinValue))
            {
                throw new InvalidRequestException();
            }

            application.Submit(command.DateSubmitted, command.SubmittedByEmail, command.SubmittedByName);

            await _domainRepository.Save(application);
        }
    }
}

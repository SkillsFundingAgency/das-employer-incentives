using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UpdateIncentiveApplication
{
    public class UpdateIncentiveApplicationCommandHandler : ICommandHandler<UpdateIncentiveApplicationCommand>
    {
        private readonly IIncentiveApplicationFactory _domainFactory;
        private readonly IIncentiveApplicationDomainRepository _domainRepository;

        public UpdateIncentiveApplicationCommandHandler(IIncentiveApplicationFactory domainFactory, IIncentiveApplicationDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(UpdateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            var application = await _domainRepository.Find(command.IncentiveApplicationId);

            foreach (var apprenticeship in RemovedApprenticeships(command, application))
            {
                application.RemoveApprenticeship(apprenticeship);
            }

            foreach (var apprenticeship in AddedApprenticeships(command, application))
            {
                application.AddApprenticeship(apprenticeship);
            }

            await _domainRepository.Save(application);
        }

        private static IEnumerable<Apprenticeship> RemovedApprenticeships(UpdateIncentiveApplicationCommand command, IncentiveApplication application)
        {
            return application.Apprenticeships.Where(x => !command.Apprenticeships.Select(a => a.Id).Contains(x.Id)).ToList();
        }

        private IEnumerable<Apprenticeship> AddedApprenticeships(UpdateIncentiveApplicationCommand command, IncentiveApplication application)
        {
            return command.Apprenticeships.Where(a =>
                !application.Apprenticeships.Select(x => x.Id).Contains(a.Id)).Select(
                apprenticeship => _domainFactory.CreateNewApprenticeship(apprenticeship.Id,
                    apprenticeship.ApprenticeshipId, apprenticeship.FirstName, apprenticeship.LastName,
                    apprenticeship.DateOfBirth, apprenticeship.Uln, apprenticeship.PlannedStartDate,
                    apprenticeship.ApprenticeshipEmployerTypeOnApproval)
            ).ToList();
        }
    }
}

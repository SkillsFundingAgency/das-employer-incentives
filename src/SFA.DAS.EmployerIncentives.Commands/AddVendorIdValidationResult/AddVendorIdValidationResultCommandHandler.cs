using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication;
using SFA.DAS.EmployerIncentives.Commands.Extensions;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.AddVendorIdValidationResult
{
    public class AddVendorIdValidationResultCommandHandler : ICommandHandler<AddVendorIdValidationResultCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _domainFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;

        public AddVendorIdValidationResultCommandHandler(IApprenticeshipIncentiveFactory domainFactory, IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(AddVendorIdValidationResultCommand command, CancellationToken cancellationToken = default)
        {
            //var application = _domainFactory.GetExisting().CreateNew(command.IncentiveApplicationId, command.AccountId, command.AccountLegalEntityId);
            //application.SetApprenticeships(command.Apprenticeships.ToEntities(_domainFactory));

            //await _domainRepository.Save(application);
        }

    }
}

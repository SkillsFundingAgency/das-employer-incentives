using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.AddValidationResults
{
    public class AddValidationResultsCommandHandler : ICommandHandler<AddValidationResultsCommand>
    {
        private readonly IApprenticeshipIncentiveFactory _domainFactory;
        private readonly IApprenticeshipIncentiveDomainRepository _domainRepository;

        public AddValidationResultsCommandHandler(IApprenticeshipIncentiveFactory domainFactory, IApprenticeshipIncentiveDomainRepository domainRepository)
        {
            _domainFactory = domainFactory;
            _domainRepository = domainRepository;
        }

        public async Task Handle(AddValidationResultsCommand command, CancellationToken cancellationToken = default)
        {
        }
    }
}

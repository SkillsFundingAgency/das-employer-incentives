using System;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.CreateIncentiveApplication
{
    public class CreateIncentiveApplicationCommandHandler : ICommandHandler<CreateIncentiveApplicationCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public CreateIncentiveApplicationCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(CreateIncentiveApplicationCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

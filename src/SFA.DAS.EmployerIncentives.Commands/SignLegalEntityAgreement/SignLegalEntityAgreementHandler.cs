using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{
    public class SignLegalEntityAgreementCommandHandler : ICommandHandler<SignLegalEntityAgreementCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;

        public SignLegalEntityAgreementCommandHandler(IAccountDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        public async Task Handle(SignLegalEntityAgreementCommand command, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

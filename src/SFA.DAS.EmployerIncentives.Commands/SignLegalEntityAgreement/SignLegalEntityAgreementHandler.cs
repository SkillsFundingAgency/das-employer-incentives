using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;

namespace SFA.DAS.EmployerIncentives.Commands.SignLegalEntityAgreement
{
    public class SignLegalEntityAgreementCommandHandler : ICommandHandler<SignLegalEntityAgreementCommand>
    {
        private readonly IAccountDomainRepository _domainRepository;
        private int _minimumAgreementVersion;

        public SignLegalEntityAgreementCommandHandler(IAccountDomainRepository domainRepository, IOptions<ApplicationSettings> options)
        {
            _domainRepository = domainRepository;
            _minimumAgreementVersion = options.Value.MinimumAgreementVersion;
        }

        public async Task Handle(SignLegalEntityAgreementCommand command, CancellationToken cancellationToken = default)
        {
            var account = await _domainRepository.Find(command.AccountId);
            var legalEntity = account.GetLegalEntity(command.AccountLegalEntityId);

            legalEntity.SignedAgreement(command.AgreementVersion, _minimumAgreementVersion);

            await _domainRepository.Save(account);
        }
    }
}

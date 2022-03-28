using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using SFA.DAS.EmployerIncentives.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Commands.AccountVrfCaseStatus
{
    public class AccountVrfCaseStatusRemindersCommandHandler : ICommandHandler<AccountVrfCaseStatusRemindersCommand>
    {
        private readonly IAccountDataRepository _accountDataRepository;
        private readonly IApprenticeApplicationDataRepository _applicationDataRepository;
        private readonly ICommandDispatcher _commandDispatcher;

        public AccountVrfCaseStatusRemindersCommandHandler(IAccountDataRepository accountDataRepository,
                                                           IApprenticeApplicationDataRepository applicationDataRepository,
                                                           ICommandDispatcher commandDispatcher)
        {
            _accountDataRepository = accountDataRepository;
            _applicationDataRepository = applicationDataRepository;
            _commandDispatcher = commandDispatcher;
        }

        public async Task Handle(AccountVrfCaseStatusRemindersCommand command, CancellationToken cancellationToken = default)
        {
            var accountsWithoutVrfStatus = await _accountDataRepository.GetByVrfCaseStatus(null);

            foreach(var account in accountsWithoutVrfStatus)
            {
                var applications = new List<ApprenticeApplication>();

                foreach (var legalEntity in account.LegalEntities)
                {
                    var applicationsForLegalEntity = await _applicationDataRepository.GetList(account.AccountId, legalEntity.AccountLegalEntityId);
                    applications.AddRange(applicationsForLegalEntity);
                }

                var submittedApplications = applications.Where(x => x.ApplicationDate < command.ApplicationCutOffDate)
                                                              .OrderBy(x => x.ApplicationDate);

                if (submittedApplications.Any())
                {
                    var application = submittedApplications.First();

                    var firstSubmittedApplicationId = await _applicationDataRepository.GetFirstSubmittedApplicationId(application.AccountLegalEntityId);
                    var sendRepeatReminderEmailCommand = new SendBankDetailsRepeatReminderEmailCommand(application.AccountId,
                                                                                                       application.AccountLegalEntityId,
                                                                                                       firstSubmittedApplicationId.Value,
                                                                                                       application.SubmittedByEmail);

                    await _commandDispatcher.Send(sendRepeatReminderEmailCommand);
                }
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.SendEmail;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailCommandController : ApiCommandControllerBase
    {
        public EmailCommandController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpPost]
        [Route("bank-details-required")]
        public async Task SendBankDetailRequiredEmail([FromBody] SendBankDetailsEmailRequest request)
        {
            await SendCommandAsync(new SendBankDetailsRequiredEmailCommand(request.AccountId,
                                                                   request.AccountLegalEntityId,
                                                                   request.EmailAddress,
                                                                   request.AddBankDetailsUrl));
        }

        [HttpPost]
        [Route("bank-details-reminder")]
        public async Task SendBankDetailReminderEmail([FromBody] SendBankDetailsEmailRequest request)
        {
            await SendCommandAsync(new SendBankDetailsReminderEmailCommand(request.AccountId,
                                                                   request.AccountLegalEntityId,
                                                                   request.EmailAddress,
                                                                   request.AddBankDetailsUrl));
        }

        [HttpPost]
        [Route("bank-details-repeat-reminder")]
        public async Task SendBankDetailsRepeatReminderEmail([FromBody] BankDetailsReminderEmailRequest request)
        {
            await SendCommandAsync(new SendBankDetailsRepeatReminderEmailCommand(request.AccountId,
                                                                                 request.AccountLegalEntityId,
                                                                                 request.ApplicationId,
                                                                                 request.EmailAddress));
        }
    }
}

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
        [Route("bank-details")]
        public async Task SendBankDetailsEmail([FromBody] SendBankDetailsEmailRequest request)
        {
            await SendCommandAsync(new SendBankDetailsEmailCommand(request.AccountId, 
                                                                   request.AccountLegalEntityId, 
                                                                   request.EmailAddress,
                                                                   request.AddBankDetailsUrl));
        }
        
    }
}

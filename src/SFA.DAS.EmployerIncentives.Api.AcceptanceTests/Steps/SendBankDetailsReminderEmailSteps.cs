using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.Notifications.Messages.Commands;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendBankDetailsReminderEmail")]
    public class SendBankDetailsReminderEmailSteps : StepsBase
    {
        private readonly Fixture _fixture;
        private readonly string _url;
        private SendBankDetailsEmailRequest _request;
        private HttpResponseMessage _response;

        public SendBankDetailsReminderEmailSteps(TestContext context) : base(context)
        {
            _fixture = new Fixture();
            _url = "/api/EmailCommand/bank-details-reminder";
        }

        [When(@"an employer selects to start the external bank details journey")]
        public async Task WhenAnEmployerSelectsToStartTheExternalBankDetailsJourney()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();

            _response = await EmployerIncentiveApi.Post(_url, _request);
        }

        [Then(@"the employer is sent a reminder email to supply their bank details with a link in case they do not complete the journey")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetailsWithALinkInCaseTheyDoNotCompleteTheJourney()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
            var command = TestContext.EventsPublished.SingleOrDefault(e => e is SendEmailCommand) as SendEmailCommand;
            Debug.Assert(command != null, nameof(command) + " != null");
            command.RecipientsAddress.Should().Be(_request.EmailAddress);
            command.TemplateId.Should().Be(EmailTemplateIds.BankDetailsReminder);
            command.Tokens["bank details url"].Should().Be(_request.AddBankDetailsUrl);
        }

        [When(@"a bank details required email is sent with an invalid email address")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidEmailAddress()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.EmailAddress = null;

            _response = await EmployerIncentiveApi.Post(_url, _request);
        }

        [Then(@"the email is not set and an error response returned")]
        public void ThenTheEmailIsNotSetAndAnErrorResponseReturned()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [When(@"A bank details required email is sent with an invalid account id")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountId()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AccountId = 0;

            _response = await EmployerIncentiveApi.Post(_url, _request);
        }

        [When(@"a bank details required email is sent with an invalid account legal entity id")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountLegalEntityId()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AccountLegalEntityId = 0;

            _response = await EmployerIncentiveApi.Post(_url, _request);
        }

        [When(@"a bank details required email is sent with an invalid account bank details url")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountBankDetailsUrl()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AddBankDetailsUrl = null;

            _response = await EmployerIncentiveApi.Post(_url, _request);
        }
    }
}

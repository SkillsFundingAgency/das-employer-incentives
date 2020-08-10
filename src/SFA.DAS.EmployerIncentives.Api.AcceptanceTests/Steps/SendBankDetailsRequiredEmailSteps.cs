using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.Notifications.Messages.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendBankDetailsRequiredEmail")]
    public class SendBankDetailsRequiredEmailSteps : StepsBase
    {
        private TestContext _testContext;
        private Fixture _fixture;
        private SendBankDetailsEmailRequest _request;
        private string _url;

        public SendBankDetailsRequiredEmailSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _url = "/api/EmailCommand/bank-details-required";
        }

        [When(@"a bank details required email is sent for a valid account, legal entity and email address")]
        public async Task WhenABankDetailsRequiredEmailIsSentForAValidAccountLegalEntityAndEmailAddress()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

        [Then(@"the employer is sent a reminder email to supply their bank details")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetails()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [When(@"a bank details required email is sent with an invalid email address")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidEmailAddress()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.EmailAddress = null;

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

        [Then(@"the email is not set and an error response returned")]
        public void ThenTheEmailIsNotSetAndAnErrorResponseReturned()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [When(@"A bank details required email is sent with an invalid account id")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountId()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AccountId = 0;

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

        [When(@"a bank details required email is sent with an invalid account legal entity id")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountLegalEntityId()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AccountLegalEntityId = 0;

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

        [When(@"a bank details required email is sent with an invalid account bank details url")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountBankDetailsUrl()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AddBankDetailsUrl = null;

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

    }
}

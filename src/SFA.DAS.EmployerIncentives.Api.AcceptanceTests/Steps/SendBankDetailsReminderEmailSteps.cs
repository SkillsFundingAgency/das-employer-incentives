using System;
using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Api.Types;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendBankDetailsReminderEmail")]
    public class SendBankDetailsReminderEmailSteps : StepsBase
    {
        private TestContext _testContext;
        private Fixture _fixture;
        private SendBankDetailsEmailRequest _request;
        private string _url;
        private string _storageDirectory;

        public SendBankDetailsReminderEmailSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _url = "/api/EmailCommand/bank-details-reminder";
            var projectDirectory = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("bin"));
            var sourceDirectory = new DirectoryInfo(projectDirectory).Parent;
            _storageDirectory = Path.Combine(sourceDirectory.FullName, ".learningtransport");
        }

        [When(@"an employer selects to start the external bank details journey")]
        public async Task WhenAnEmployerSelectsToStartTheExternalBankDetailsJourney()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();

            await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request);
        }

        [Then(@"the employer is sent a reminder email to supply their bank details with a link in case they do not complete the journey")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetailsWithALinkInCaseTheyDoNotCompleteTheJourney()
        {
            EmployerIncentiveApi.Response.StatusCode.Should().Be(HttpStatusCode.OK);

            var directoryInfo = new DirectoryInfo($"{_storageDirectory}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
            var recentFiles = directoryInfo.GetFiles().OrderByDescending(x => x.CreationTimeUtc >= DateTime.Now.AddMinutes(-2));

            foreach (var file in recentFiles)
            {
                var contents = File.ReadAllText(file.FullName, Encoding.UTF8);

                if (contents.Contains(_request.EmailAddress) && contents.Contains(_request.AddBankDetailsUrl))
                {
                    return;
                }
            }

            Assert.Fail($"No NServiceBus Message found with {_request.EmailAddress} and {_request.AddBankDetailsUrl}");
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

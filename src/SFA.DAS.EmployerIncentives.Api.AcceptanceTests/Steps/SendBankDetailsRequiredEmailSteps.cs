using AutoFixture;
using FluentAssertions;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Types;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "SendBankDetailsRequiredEmail")]
    public class SendBankDetailsRequiredEmailSteps : StepsBase
    {
        private readonly TestContext _testContext;
        private readonly Fixture _fixture;
        private SendBankDetailsEmailRequest _request;
        private readonly string _url;
        private readonly string _storageDirectory;
        private HttpResponseMessage _response;

        public SendBankDetailsRequiredEmailSteps(TestContext testContext) : base(testContext)
        {
            _testContext = testContext;
            _fixture = new Fixture();
            _url = "/api/EmailCommand/bank-details-required";
            _storageDirectory = testContext.MessageBus.StorageDirectory.FullName;
        }

        [When(@"a bank details required email is sent for a valid account, legal entity and email address")]
        public async Task WhenABankDetailsRequiredEmailIsSentForAValidAccountLegalEntityAndEmailAddress()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();

            await _testContext.WaitFor<ICommand>(async (cancellationToken) =>
            {
                _response = await EmployerIncentiveApi.Post(_url, _request, cancellationToken);
            });            
        }

        [Then(@"the employer is sent a reminder email to supply their bank details")]
        public void ThenTheEmployerIsSentAReminderEmailToSupplyTheirBankDetails()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);

            var directoryInfo = new DirectoryInfo($"{_storageDirectory}\\SFA.DAS.Notifications.MessageHandlers\\.bodies\\");
            var recentFiles = directoryInfo.GetFiles().OrderByDescending(x => x.CreationTimeUtc >= DateTime.Now.AddMinutes(-2));

            foreach (var file in recentFiles)
            {
                var contents = File.ReadAllText(file.FullName, Encoding.UTF8);

                if(contents.Contains(_request.EmailAddress) && contents.Contains(_request.AddBankDetailsUrl))
                {
                    return;
                }
            }

            throw new Exception($"No NServiceBus Message found with {_request.EmailAddress} and {_request.AddBankDetailsUrl}");
        }

        [When(@"a bank details required email is sent with an invalid email address")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidEmailAddress()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.EmailAddress = null;

            _response = await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request, TestContext.CancellationToken);
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

            _response = await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request, TestContext.CancellationToken);
        }

        [When(@"a bank details required email is sent with an invalid account legal entity id")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountLegalEntityId()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AccountLegalEntityId = 0;

            _response = await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request, TestContext.CancellationToken);
        }

        [When(@"a bank details required email is sent with an invalid account bank details url")]
        public async Task WhenABankDetailsRequiredEmailIsSentWithAnInvalidAccountBankDetailsUrl()
        {
            _request = _fixture.Create<SendBankDetailsEmailRequest>();
            _request.AddBankDetailsUrl = null;

            _response = await EmployerIncentiveApi.Post<SendBankDetailsEmailRequest>(_url, _request, TestContext.CancellationToken);
        }

    }
}

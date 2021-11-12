using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    [TestFixture]
    public class WhenRunningSendClawbacksForAccountLegalEntityOrchestrator
    {
        private Fixture _fixture;
        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriodInput;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private SendClawbacksForAccountLegalEntityOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _accountLegalEntityCollectionPeriodInput = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<AccountLegalEntityCollectionPeriod>()).Returns(_accountLegalEntityCollectionPeriodInput);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("SendClawbacksForAccountLegalEntity", _accountLegalEntityCollectionPeriodInput)).ReturnsAsync(true);
            _orchestrator = new SendClawbacksForAccountLegalEntityOrchestrator(Mock.Of<ILogger<SendClawbacksForAccountLegalEntityOrchestrator>>());
        }

        [Test]
        public async Task Then_the_clawbacks_are_sent_to_business_central_for_the_account_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendClawbacksForAccountLegalEntity", _accountLegalEntityCollectionPeriodInput), Times.Once);
        }
    }
}

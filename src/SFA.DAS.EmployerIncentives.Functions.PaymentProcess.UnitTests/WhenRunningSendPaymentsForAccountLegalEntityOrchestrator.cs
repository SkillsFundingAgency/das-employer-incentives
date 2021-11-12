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
    public class WhenRunningSendPaymentsForAccountLegalEntityOrchestrator
    {
        private Fixture _fixture;
        private AccountLegalEntityCollectionPeriod _accountLegalEntityCollectionPeriodInput;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private SendPaymentsForAccountLegalEntityOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _accountLegalEntityCollectionPeriodInput = _fixture.Create<AccountLegalEntityCollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<AccountLegalEntityCollectionPeriod>()).Returns(_accountLegalEntityCollectionPeriodInput);

            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<bool>("SendPaymentRequestsForAccountLegalEntity", _accountLegalEntityCollectionPeriodInput)).ReturnsAsync(true);
            _orchestrator = new SendPaymentsForAccountLegalEntityOrchestrator(Mock.Of<ILogger<SendPaymentsForAccountLegalEntityOrchestrator>>());
        }

        [Test]
        public async Task Then_the_payments_are_sent_to_business_central_for_the_account_legal_entity()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", _accountLegalEntityCollectionPeriodInput), Times.Once);
        }
    }
}

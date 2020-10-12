using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenRunningPaymentsProcess
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private IncentivePaymentOrchestrator _orchestrator;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockOrchestrationContext.Setup(x => x.GetInput<CollectionPeriod>()).Returns(_collectionPeriod);

            var legalEntities = _fixture.Create<List<long>>();
            _mockOrchestrationContext.Setup(x => x.CallActivityAsync<List<long>>("GetPayableLegalEntities", _collectionPeriod)).ReturnsAsync(legalEntities);

            _orchestrator = new IncentivePaymentOrchestrator(Mock.Of<ILogger<IncentivePaymentOrchestrator>>());
        }

        [Test]
        public async Task Then_query_is_called_to_get_payable_legal_entities()
        {
            await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<long>>("GetPayableLegalEntities", _collectionPeriod), Times.Once);
        }
    }
}
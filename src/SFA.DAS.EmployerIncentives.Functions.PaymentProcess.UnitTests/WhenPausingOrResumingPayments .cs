using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Functions.TestHelpers;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenPausingOrResumingPayments
    {
        private Fixture _fixture;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Mock<ILogger> _mockLogger;
        private HandlePausePaymentsRequest _sut;
        private PausePaymentsRequest _request;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();

            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _mockLogger = new Mock<ILogger>();

            _sut = new HandlePausePaymentsRequest(_mockCommandDispatcher.Object);
        }

        //[Test]
        //public async Task Then_PausePaymentsCommand_is_created_and_dispatched()
        //{
        //    var _request = _fixture.Create<PausePaymentsRequest>();
        //    var httpRequest = new DummyHttpRequest(JsonConvert.SerializeObject(_request));

        //    await _sut.Run(httpRequest, 999, _mockLogger.Object);

        //    _mockOrchestrationContext.Verify(x => x.CallActivityAsync<List<PayableLegalEntityDto>>("GetPayableLegalEntities", _collectionPeriod), Times.Once);
        //}

        //[Test]
        //public async Task Then_sub_orchestrator_is_called_to_calculate_payments_for_each_legal_entity()
        //{
        //    await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

        //    _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
        //        "CalculatePaymentsForAccountLegalEntityOrchestrator",
        //        It.IsAny<AccountLegalEntityCollectionPeriod>()
        //    ), Times.Exactly(_legalEntities.Count));

        //    foreach (var entity in _legalEntities)
        //    {
        //        _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
        //                "CalculatePaymentsForAccountLegalEntityOrchestrator",
        //                It.Is<AccountLegalEntityCollectionPeriod>(input =>
        //                    input.AccountLegalEntityId == entity.AccountLegalEntityId &&
        //                    input.AccountId == entity.AccountId &&
        //                    input.CollectionPeriod.Period == _collectionPeriod.Period &&
        //                    input.CollectionPeriod.Year == _collectionPeriod.Year)

        //            ), Times.Once);
        //    }
        //}

        //[Test]
        //public async Task Then_process_pauses_after_generating_payments()
        //{
        //    await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

        //    _mockOrchestrationContext.Verify(x => x.CallSubOrchestratorAsync(
        //        "CalculatePaymentsForAccountLegalEntityOrchestrator",
        //        It.IsAny<AccountLegalEntityCollectionPeriod>()
        //    ), Times.Exactly(_legalEntities.Count));

        //    _mockOrchestrationContext.Verify(x => x.WaitForExternalEvent<bool>("PaymentsApproved"), Times.Once);
        //}

        //[Test]
        //public async Task Then_payments_are_not_sent_if_not_approved()
        //{
        //    _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(false);

        //    await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

        //    _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Never);
        //}

        //[Test]
        //public async Task Then_payments_are_sent_when_they_are_approved()
        //{
        //    _mockOrchestrationContext.Setup(x => x.WaitForExternalEvent<bool>("PaymentsApproved")).ReturnsAsync(true);

        //    await _orchestrator.RunOrchestrator(_mockOrchestrationContext.Object);

        //    _mockOrchestrationContext.Verify(x => x.CallActivityAsync("SendPaymentRequestsForAccountLegalEntity", It.IsAny<AccountLegalEntityCollectionPeriod>()), Times.Exactly(3));

        //    foreach (var entity in _legalEntities)
        //    {
        //        _mockOrchestrationContext.Verify(x => x.CallActivityAsync(
        //            "SendPaymentRequestsForAccountLegalEntity",
        //            It.Is<AccountLegalEntityCollectionPeriod>(input =>
        //                input.AccountLegalEntityId == entity.AccountLegalEntityId &&
        //                input.AccountId == entity.AccountId &&
        //                input.CollectionPeriod.Period == _collectionPeriod.Period &&
        //                input.CollectionPeriod.Year == _collectionPeriod.Year)

        //        ), Times.Once);
        //    }
        //}
    }
}
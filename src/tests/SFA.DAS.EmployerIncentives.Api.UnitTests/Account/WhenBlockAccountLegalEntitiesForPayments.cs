using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.VendorBlock;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Account
{
    [TestFixture]
    public class WhenBlockAccountLegalEntitiesForPayments
    {
        private AccountCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new AccountCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<BlockAccountLegalEntityForPaymentsCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_block_account_legal_entity_for_payments_command_is_dispatched_for_each_vendor_block_request()
        {
            // Arrange
            var request = _fixture.Build<BlockAccountLegalEntityForPaymentsRequest>()
                .With(x => x.VendorBlocks, _fixture.CreateMany<VendorBlock>(10).ToList())
                .Create();

            var requestList = new List<BlockAccountLegalEntityForPaymentsRequest>
            {
                request
            };

            // Act
            await _sut.BlockAccountLegalEntityForPayments(requestList);

            // Assert
            foreach(var vendorBlock in request.VendorBlocks)
            {
                _mockCommandDispatcher.Verify(m => m.Send(It.Is<BlockAccountLegalEntityForPaymentsCommand>(
                    c => c.VendorId == vendorBlock.VendorId
                    && c.VendorBlockEndDate == vendorBlock.VendorBlockEndDate
                    && c.ServiceRequestTaskId == request.ServiceRequest.TaskId
                    && c.ServiceRequestDecisionReference == request.ServiceRequest.DecisionReference
                    && c.ServiceRequestCreatedDate == request.ServiceRequest.TaskCreatedDate), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

    }
}

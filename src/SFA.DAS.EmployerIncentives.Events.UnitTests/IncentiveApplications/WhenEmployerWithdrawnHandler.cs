using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.IncentiveApplications
{
    public class WhenEmployerWithdrawnHandler
    {
        private EmployerWithdrawnHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _sut = new EmployerWithdrawnHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_WithdrawCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<EmployerWithdrawn>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockCommandPublisher.Verify(m =>
            m.Publish(It.Is<WithdrawCommand>(i =>
                   i.AccountId == @event.AccountId &&
                   i.IncentiveApplicationApprenticeshipId == @event.Model.Id &&
                   i.WithdrawnBy == WithdrawnBy.Employer
               ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

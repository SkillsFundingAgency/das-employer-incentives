using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.IncentiveApplications
{
    public class WhenSubmittedHandler
    {
        private SubmittedHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _sut = new SubmittedHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_CalculateEarningsCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<Submitted>();

            //Act
            await _sut.Handle(@event);

            //Assert
            foreach (var apprenticeship in @event.Model.ApprenticeshipModels)
            {
                _mockCommandPublisher.Verify(m => m.Publish(It.Is<CreateIncentiveCommand>(i =>
                    i.AccountId == @event.Model.AccountId &&
                    i.AccountLegalEntityId == @event.Model.AccountLegalEntityId &&
                    i.IncentiveApplicationApprenticeshipId == apprenticeship.Id &&
                    i.ApprenticeshipId == apprenticeship.ApprenticeshipId &&
                    i.FirstName == apprenticeship.FirstName &&
                    i.LastName == apprenticeship.LastName &&
                    i.DateOfBirth == apprenticeship.DateOfBirth &&
                    i.Uln == apprenticeship.ULN &&
                    i.PlannedStartDate == apprenticeship.PlannedStartDate &&
                    i.ApprenticeshipEmployerTypeOnApproval == apprenticeship.ApprenticeshipEmployerTypeOnApproval &&
                    i.UKPRN == apprenticeship.UKPRN
                ), It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}

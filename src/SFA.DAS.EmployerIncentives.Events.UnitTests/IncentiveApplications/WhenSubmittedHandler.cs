using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Linq;
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
        public async Task Then_a_CreateIncentiveCommand_is_published()
        {
            //Arrange
            var @event = _fixture.Create<Submitted>();

            //Act
            await _sut.Handle(@event);

            //Assert
            @event.Model.ApprenticeshipModels.Count.Should().BeGreaterThan(0);
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

        [Test]
        public async Task Then_a_CreateIncentiveCommand_is_not_published_for_withdrawn_apprenticeships()
        {
            //Arrange
            var apprenticeships = _fixture.CreateMany<ApprenticeshipModel>(5).ToList();
            apprenticeships[0].WithdrawnByEmployer = true;
            apprenticeships[1].WithdrawnByEmployer = false;
            apprenticeships[2].WithdrawnByEmployer = false;
            apprenticeships[3].WithdrawnByEmployer = true;
            apprenticeships[4].WithdrawnByEmployer = true;

            var model = _fixture.Build<IncentiveApplicationModel>()
                .With(x => x.ApprenticeshipModels, apprenticeships).Create();
            var @event = new Submitted(model);

            //Act
            await _sut.Handle(@event);

            //Assert
            foreach (var apprenticeship in @event.Model.ApprenticeshipModels.Where(a => a.WithdrawnByEmployer == false))
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
                    i.ApprenticeshipEmployerTypeOnApproval == apprenticeship.ApprenticeshipEmployerTypeOnApproval
                ), It.IsAny<CancellationToken>()), Times.Once);
            }

            foreach (var apprenticeship in @event.Model.ApprenticeshipModels.Where(a => a.WithdrawnByEmployer == true))
            {
                _mockCommandPublisher.Verify(m => m.Publish(It.Is<CreateIncentiveCommand>(i =>
                    i.IncentiveApplicationApprenticeshipId == apprenticeship.Id
                ), It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
}

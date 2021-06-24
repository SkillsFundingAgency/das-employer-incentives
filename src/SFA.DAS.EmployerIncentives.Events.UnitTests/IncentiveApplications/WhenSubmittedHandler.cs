using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Events.IncentiveApplications;
using System.Collections.Generic;
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
            foreach (var apprenticeship in @event.EligibleApprenticeships())
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
                    i.UKPRN == apprenticeship.UKPRN &&
                    i.SubmittedDate == @event.Model.DateSubmitted &&
                    i.SubmittedByEmail == @event.Model.SubmittedByEmail &&
                    i.CourseName == apprenticeship.CourseName &&
                    i.EmploymentStartDate == apprenticeship.EmploymentStartDate &&
                    i.Phase == apprenticeship.Phase
                ), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestCase(false, false, true, true)]
        [TestCase(false, false, false, false)]
        [TestCase(false, true, true, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, true, false, false)]
        public async Task Then_a_CreateIncentiveCommand_is_published_for_eligible_apprenticeships(
            bool withdrawnByEmployer, bool withdrawnByCompliance, bool hasEligibleEmploymentStartDate, bool accepted)
        {
            //Arrange
            var apprenticeship = _fixture.Create<ApprenticeshipModel>();
            apprenticeship.WithdrawnByEmployer = withdrawnByEmployer;
            apprenticeship.WithdrawnByCompliance = withdrawnByCompliance;
            apprenticeship.HasEligibleEmploymentStartDate = hasEligibleEmploymentStartDate;

            var model = _fixture.Build<IncentiveApplicationModel>()
                .With(x => x.ApprenticeshipModels,
                    new List<ApprenticeshipModel> {apprenticeship}).Create();

            var @event = new Submitted(model);

            //Act
            await _sut.Handle(@event);

            //Assert
            if (accepted)
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
                    i.Phase == apprenticeship.Phase
                ), It.IsAny<CancellationToken>()), Times.Once);
            }
            else
            {
                _mockCommandPublisher.Verify(m => m.Publish(It.Is<CreateIncentiveCommand>(i =>
                    i.IncentiveApplicationApprenticeshipId == apprenticeship.Id
                ), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

    }
}

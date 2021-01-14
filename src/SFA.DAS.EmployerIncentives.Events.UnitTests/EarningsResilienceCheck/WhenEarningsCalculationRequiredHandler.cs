using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Events.EarningsResilienceCheck;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.EarningsResilienceCheck
{
    public class WhenEarningsCalculationRequiredHandler
    {
        private EarningsCalculationRequiredHandler _sut;
        private Mock<ICommandPublisher> _mockCommandPublisher;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockCommandPublisher = new Mock<ICommandPublisher>();

            _sut = new EarningsCalculationRequiredHandler(_mockCommandPublisher.Object);
        }

        [Test]
        public async Task Then_a_create_apprenticeship_incentive_command_is_published_for_each_apprenticeship()
        {
            //Arrange
            var @event = _fixture.Create<EarningsCalculationRequired>();
            @event.Model.ApprenticeshipModels = new Collection<ApprenticeshipModel>(_fixture.CreateMany<ApprenticeshipModel>(5).ToList());

            //Act
            await _sut.Handle(@event);

            //Assert
            foreach(var apprenticeship in @event.Model.ApprenticeshipModels)
            {
                _mockCommandPublisher.Verify(x => x.Publish(It.Is<CreateIncentiveCommand>
                    (
                        x => x.ApprenticeshipId == apprenticeship.ApprenticeshipId &&
                        x.ApprenticeshipEmployerTypeOnApproval == apprenticeship.ApprenticeshipEmployerTypeOnApproval &&
                        x.DateOfBirth == apprenticeship.DateOfBirth &&
                        x.FirstName == apprenticeship.FirstName &&
                        x.IncentiveApplicationApprenticeshipId == apprenticeship.Id &&
                        x.LastName == apprenticeship.LastName &&
                        x.PlannedStartDate == apprenticeship.PlannedStartDate &&
                        x.UKPRN == apprenticeship.UKPRN &&
                        x.Uln == apprenticeship.ULN &&
                        x.SubmittedDate == @event.Model.DateSubmitted &&
                        x.SubmittedByEmail == @event.Model.SubmittedByEmail
                    ),
                    It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}

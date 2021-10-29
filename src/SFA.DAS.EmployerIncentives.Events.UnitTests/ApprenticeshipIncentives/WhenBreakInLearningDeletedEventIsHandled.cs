using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenBreakInLearningDeletedEventIsHandled
    {
        private BreakInLearningDeletedHandler _sut;
        private Mock<ILearnerDataRepository> _mockDataRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockDataRepository = new Mock<ILearnerDataRepository>();
            _sut = new BreakInLearningDeletedHandler(_mockDataRepository.Object);
        }

        [Test]
        public async Task Then_a_LearningStoppedDate_is_set_to_null()
        {
            //Arrange
            var @event = _fixture.Create<BreakInLearningDeleted>();
            var learner = _fixture.Build<LearnerModel>()
                .With(x => x.ApprenticeshipIncentiveId, @event.ApprenticeshipIncentiveId)
                .Without(l => l.LearningPeriods)
                .Create();
            _mockDataRepository.Setup(x => x.GetByApprenticeshipIncentiveId(@event.ApprenticeshipIncentiveId))
                .ReturnsAsync(learner);
           
            //Act
            await _sut.Handle(@event);

            //Assert
            _mockDataRepository.Verify(m =>
                    m.Update(It.Is<LearnerModel>(i =>
                        i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                        i.SubmissionData.LearningData.StoppedStatus.DateResumed == null &&
                        i.SubmissionData.LearningData.StoppedStatus.DateStopped == null
                    )),
                Times.Once);
        }
    }
}

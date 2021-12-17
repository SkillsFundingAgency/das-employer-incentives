using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenBreakInLearningDeletedCoCEventIsHandled
    {
        private BreakInLearningDeletedCoCHandler _sut;
        private Mock<IChangeOfCircumstancesDataRepository> _mockChangeOfCircumstancesDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockChangeOfCircumstancesDataRepository = new Mock<IChangeOfCircumstancesDataRepository>();

            _sut = new BreakInLearningDeletedCoCHandler(_mockChangeOfCircumstancesDataRepository.Object);
        }

        [Test]
        public async Task Then_a_ChangeOfCircumstance_is_persisted()
        {
            //Arrange
            var @event = _fixture.Create<BreakInLearningDeleted>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockChangeOfCircumstancesDataRepository.Verify(m =>
            m.Save(It.Is<ChangeOfCircumstance>(i =>
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.Type == Enums.ChangeOfCircumstanceType.BreakInLearningDel &&
                   i.NewValue == string.Empty &&
                   i.PreviousValue == string.Empty &&
                   i.ChangedDate == DateTime.Today
                   )),
                   Times.Once);
        }
    }
}

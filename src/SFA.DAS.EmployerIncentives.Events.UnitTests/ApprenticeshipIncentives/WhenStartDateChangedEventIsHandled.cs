using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenStartDateChangedEventIsHandled
    {
        private StartDateChangedHandler _sut;
        private Mock<IChangeOfCircumstancesDataRepository> _mockChangeOfCircumstancesDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockChangeOfCircumstancesDataRepository = new Mock<IChangeOfCircumstancesDataRepository>();

            _sut = new StartDateChangedHandler(_mockChangeOfCircumstancesDataRepository.Object);
        }

        [Test]
        public async Task Then_a_ChangeOfCircumstance_is_persisted()
        {
            //Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .Without(x => x.BreakInLearnings)
                .Create();

            var @event = new StartDateChanged(
                apprenticeshipIncentiveModel.Id,
                _fixture.Create<DateTime>(),
                _fixture.Create<DateTime>(),
                apprenticeshipIncentiveModel);

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockChangeOfCircumstancesDataRepository.Verify(m =>
            m.Save(It.Is<ChangeOfCircumstance>(i =>
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.Type == Enums.ChangeOfCircumstanceType.StartDate &&
                   i.NewValue == @event.NewStartDate.ToString("yyyy-MM-dd") &&
                   i.PreviousValue == @event.PreviousStartDate.ToString("yyyy-MM-dd")
                   )),
                   Times.Once);
        }
    }
}

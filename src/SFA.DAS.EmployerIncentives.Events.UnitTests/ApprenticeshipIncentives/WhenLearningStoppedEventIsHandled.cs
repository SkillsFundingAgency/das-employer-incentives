using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Events.ApprenticeshipIncentives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Events.UnitTests.ApprenticeshipIncentives
{
    public class WhenLearningStoppedEventIsHandled
    {
        private LearningStoppedHandler _sut;
        private Mock<IChangeOfCircumstancesDataRepository> _mockChangeOfCircumstancesDataRepository;

        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockChangeOfCircumstancesDataRepository = new Mock<IChangeOfCircumstancesDataRepository>();

            _mockChangeOfCircumstancesDataRepository
                .Setup(x => x.GetList(It
                    .IsAny<Expression<Func<Data.ApprenticeshipIncentives.Models.ChangeOfCircumstance, bool>>>()))
                .ReturnsAsync(new List<ChangeOfCircumstance>());

            _sut = new LearningStoppedHandler(_mockChangeOfCircumstancesDataRepository.Object);
        }

        [Test]
        public async Task Then_a_ChangeOfCircumstance_is_persisted()
        {
            //Arrange
            var @event = _fixture.Create<LearningStopped>();

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockChangeOfCircumstancesDataRepository.Verify(m =>
            m.Save(It.Is<ChangeOfCircumstance>(i =>
                   i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                   i.Type == Enums.ChangeOfCircumstanceType.LearningStopped &&
                   i.NewValue == @event.StoppedDate.ToString("yyyy-MM-dd") &&
                   i.PreviousValue == string.Empty
                   )),
                   Times.Once);
        }

        [Test]
        public async Task Then_the_previous_change_of_circumstance_date_is_persisted_if_present()
        {
            //Arrange
            var @event = _fixture.Create<LearningStopped>();

            var changesOfCircumstance = new List<ChangeOfCircumstance>
            {
                new ChangeOfCircumstance(Guid.NewGuid(), @event.ApprenticeshipIncentiveId, ChangeOfCircumstanceType.LearningStopped, "", "2022-03-01", DateTime.Now.AddMonths(-3)),
                new ChangeOfCircumstance(Guid.NewGuid(), @event.ApprenticeshipIncentiveId, ChangeOfCircumstanceType.LearningStopped, "2022-03-01", "2022-05-01", DateTime.Now.AddMonths(-1))
            };

            _mockChangeOfCircumstancesDataRepository
                .Setup(x => x.GetList(It
                    .IsAny<Expression<Func<Data.ApprenticeshipIncentives.Models.ChangeOfCircumstance, bool>>>()))
                .ReturnsAsync(changesOfCircumstance);

            //Act
            await _sut.Handle(@event);

            //Assert
            _mockChangeOfCircumstancesDataRepository.Verify(m =>
                    m.Save(It.Is<ChangeOfCircumstance>(i =>
                        i.ApprenticeshipIncentiveId == @event.ApprenticeshipIncentiveId &&
                        i.Type == Enums.ChangeOfCircumstanceType.LearningStopped &&
                        i.NewValue == @event.StoppedDate.ToString("yyyy-MM-dd") &&
                        i.PreviousValue == changesOfCircumstance[1].NewValue
                    )),
                Times.Once);
        }
    }
}

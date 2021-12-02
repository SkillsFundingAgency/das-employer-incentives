using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    [TestFixture]
    public class WhenHandlingRefreshEmploymentChecksCommand
    {
        private RefreshEmploymentChecksCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture(); 
            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCommandPublisher = new Mock<ICommandPublisher>();
            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(1, _fixture.Create<short>()), (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), true, false)
            };

            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods));
        }

        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_requested_if_the_apprentices_have_a_learning_record()
        {
            // Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .Create();
            
            var incentives =
                new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
                {
                    Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(apprenticeshipIncentiveModel.Id,
                        apprenticeshipIncentiveModel)
                };

            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(incentives);

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockCommandPublisher.Object, _mockCollectionCalendarService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockCommandPublisher.Verify(x => x.Publish(It.Is<SendEmploymentCheckRequestsCommand>(y => y.ApprenticeshipIncentiveId == incentives[0].Id), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_not_requested_if_the_apprentices_do_not_have_a_learning_record()
        {
            // Arrange
            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>());

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockCommandPublisher.Object, _mockCollectionCalendarService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockCommandPublisher.Verify(x => x.Publish(It.IsAny<SendEmploymentCheckRequestsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }


        [Test]
        public async Task Then_no_refresh_of_the_employment_checks_takes_place_when_the_active_period_is_in_progress()
        {
            // Arrange
            _collectionPeriods = new List<CollectionCalendarPeriod>()
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(1, _fixture.Create<short>()), (byte)DateTime.Today.Month, (short)DateTime.Today.Year, DateTime.Today.AddDays(-1), _fixture.Create<DateTime>(), true, true)
            };
            _mockCollectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>(), _collectionPeriods));

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockIncentiveDomainRespository.Verify(x => x.FindIncentivesWithLearningFound(), Times.Never);
            _mockCommandPublisher.Verify(x => x.Publish(It.IsAny<SendEmploymentCheckRequestsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

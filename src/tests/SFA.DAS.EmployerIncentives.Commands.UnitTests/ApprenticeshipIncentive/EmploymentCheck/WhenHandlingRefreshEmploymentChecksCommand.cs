using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.EmploymentCheck;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    [TestFixture]
    public class WhenHandlingRefreshEmploymentChecksCommand
    {
        private RefreshEmploymentChecksCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository; 
        private Mock<IDateTimeService> _mockDateTimeService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            
            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);

            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
        }

        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_requested_if_the_apprentices_have_a_learning_record()
        {
            // Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, _fixture.CreateMany<EmploymentCheckModel>(2).ToList())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();
            
            var incentives =
                new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
                {
                    Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(apprenticeshipIncentiveModel.Id,
                        apprenticeshipIncentiveModel)
                };

            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(incentives);

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockDateTimeService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockIncentiveDomainRespository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(y => y.Id == incentives[0].Id)), Times.Once);
        }


        [Test]
        public async Task Then_a_refresh_of_the_employment_check_is_not_requested_if_the_apprentices_do_not_have_a_learning_record()
        {
            // Arrange
            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>());

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockDateTimeService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockIncentiveDomainRespository.Verify(x => x.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
        }

        [Test]
        public async Task Then_the_employment_checks_are_created_if_they_do_not_exist_for_the_apprenticeship_incentive()
        {
            // Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>())
                .With(x => x.StartDate, new DateTime(2021, 10, 01))
                .With(x => x.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            var incentives =
                new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
                {
                    Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(apprenticeshipIncentiveModel.Id,
                        apprenticeshipIncentiveModel)
                };

            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(incentives);

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockDateTimeService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockIncentiveDomainRespository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(y => y.EmploymentChecks.Any())), Times.Once);
        }

        [Test]
        public async Task Then_the_employment_checks_are_not_created_if_the_start_date_is_less_than_weeks_from_todays_date()
        {
            // Arrange
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.EmploymentCheckModels, new List<EmploymentCheckModel>())
                .With(x => x.StartDate, DateTime.Now.AddDays(-41))
                .Create();

            var incentives =
                new List<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>
                {
                    Domain.ApprenticeshipIncentives.ApprenticeshipIncentive.Get(apprenticeshipIncentiveModel.Id,
                        apprenticeshipIncentiveModel)
                };

            _mockIncentiveDomainRespository.Setup(x => x.FindIncentivesWithLearningFound()).ReturnsAsync(incentives);

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockDateTimeService.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockIncentiveDomainRespository.Verify(x => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(y => y.EmploymentChecks.Any())), Times.Never);
        }
    }
}

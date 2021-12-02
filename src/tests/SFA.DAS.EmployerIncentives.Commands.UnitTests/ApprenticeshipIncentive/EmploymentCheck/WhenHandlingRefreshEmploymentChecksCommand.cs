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

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.EmploymentCheck
{
    [TestFixture]
    public class WhenHandlingRefreshEmploymentChecksCommand
    {
        private RefreshEmploymentChecksCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRespository;
        private Mock<ICommandPublisher> _mockCommandPublisher;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture(); 
            _mockIncentiveDomainRespository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockCommandPublisher = new Mock<ICommandPublisher>();
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

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockCommandPublisher.Object);

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

            _sut = new RefreshEmploymentChecksCommandHandler(_mockIncentiveDomainRespository.Object, _mockCommandPublisher.Object);

            // Act
            await _sut.Handle(new RefreshEmploymentChecksCommand());

            // Assert
            _mockCommandPublisher.Verify(x => x.Publish(It.IsAny<SendEmploymentCheckRequestsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

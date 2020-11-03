using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.ValidatePendingPayment;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Commands.Types.ApprenticeshipIncentive;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RefreshLearner.Handlers
{
    public class WhenHandlingRefreshLearnerCommand
    {
        private RefreshLearnerCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockDomainRepository;
        private Mock<ILearnerService> _mockLearnerService;
        private Mock<ILearnerDataRepository> _mockLearnerDataRepository;
        private Fixture _fixture;
        private Guid _apprenticeshipIncentiveId;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();                        

            _mockDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerService = new Mock<ILearnerService>();
            _mockLearnerDataRepository = new Mock<ILearnerDataRepository>();

            var apprenticeship = _fixture.Create<Apprenticeship>();
            apprenticeship.SetProvider(_fixture.Create<Provider>());

            var incentive = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .Create();

            _apprenticeshipIncentiveId = incentive.Id;

            _mockDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveId, incentive));

            _sut = new RefreshLearnerCommandHandler(_mockDomainRepository.Object, _mockLearnerService.Object, _mockLearnerDataRepository.Object);
        }

        [Test]
        public async Task Then_a_Learner_record_is_created_when_it_doesnt_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDataRepository.Verify(m => m.Save(It.IsAny<Learner>()), Times.Once);
        }

        [Test]
        public async Task Then_a_Learner_record_is_updated_when_it_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learner = _fixture.Create<Learner>();

            _mockLearnerDataRepository
                .Setup(m => m.GetByApprenticeshipIncentiveId(_apprenticeshipIncentiveId))
                .ReturnsAsync(learner);                

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDataRepository.Verify(m => m.Save(learner), Times.Once);
        }
    }
}

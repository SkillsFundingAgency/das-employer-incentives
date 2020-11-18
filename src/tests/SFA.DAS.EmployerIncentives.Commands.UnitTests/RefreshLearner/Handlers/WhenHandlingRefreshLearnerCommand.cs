using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RefreshLearner.Handlers
{
    public class WhenHandlingRefreshLearnerCommand
    {
        private RefreshLearnerCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockApprenticeshipIncentiveDomainRepository;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Mock<ILearnerService> _mockLearnerService;
        private ILearnerFactory _learnerFactory;
        private Fixture _fixture;
        private Guid _apprenticeshipIncentiveId;
        private LearnerSubmissionDto _learnerSubmissionDto;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockApprenticeshipIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();
            _mockLearnerService = new Mock<ILearnerService>();
            _learnerFactory = new LearnerFactory();

            var apprenticeship = _fixture.Create<Apprenticeship>();
            apprenticeship.SetProvider(_fixture.Create<Provider>());

            var incentive = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .Create();

            _apprenticeshipIncentiveId = incentive.Id;

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveId, incentive));

            _learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(_learnerSubmissionDto);

            _sut = new RefreshLearnerCommandHandler(
                _mockApprenticeshipIncentiveDomainRepository.Object, 
                _mockLearnerService.Object,
                _mockLearnerDomainRepository.Object,
                _learnerFactory);
        }

        [Test]
        public async Task Then_a_Learner_record_is_created_when_it_doesnt_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(It.IsAny<Learner>()), Times.Once);
        }

        [Test]
        public async Task Then_a_Learner_record_is_updated_when_it_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetByApprenticeshipIncentiveId(_apprenticeshipIncentiveId))
                .ReturnsAsync(learner);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(learner), Times.Once);
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_false_when_the_learner_data_does_not_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(null as LearnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionFound)
                ), Times.Once);
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_true_when_the_learner_data_does_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionFound)
                ), Times.Once);
        }

        [Test]
        public async Task Then_the_learning_found_is_false_when_there_are_no_matching_training_entries_returned_from_the_learner_service()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.LearningFound)
                ), Times.Once);
        }

        [Test]
        public async Task Then_the_learning_found_is_true_when_there_are_matching_training_entries_returned_from_the_learner_service()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture
                        .Build<LearnerSubmissionDto>()
                        .With(l => l.Training, new List<TrainingDto> {
                            _fixture.Create<TrainingDto>(),
                            _fixture
                                .Build<TrainingDto>()
                                .With(p => p.Reference, "ZPROG001")
                                .Create(),
                            _fixture.Create<TrainingDto>()
                            })
                        .Create();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningFound)
                ), Times.Once);
        }

        [Test]
        public async Task Then_the_submission_date_is_true_the_learner_data_exists()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var testDate = DateTime.Now.AddDays(-1);

            var learnerSubmissionDto = _fixture
                        .Build<LearnerSubmissionDto>()
                        .With(l => l.IlrSubmissionDate, testDate)
                        .Create();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.SubmissionDate == testDate)
                ), Times.Once);
        }
    }
}

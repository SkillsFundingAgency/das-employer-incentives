﻿using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Application.UnitTests;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RefreshLearner.Handlers
{
    public class WhenHandlingRefreshLearnerCommand
    {
        private RefreshLearnerCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockApprenticeshipIncentiveDomainRepository;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Mock<ILearnerService> _mockLearnerService;
        private Mock<ILogger<RefreshLearnerCommandHandler>> _mockLogger;
        private Mock<ICollectionCalendarService> _collectionCalendarService;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Fixture _fixture;
        private Guid _apprenticeshipIncentiveId;
        private LearnerServiceResponse _learnerServiceResponse;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _apprenticeshipIncentive;
        private ApprenticeshipIncentiveModel _incentiveModel;
        private Learner _learner;
        private DateTime _testStartDate;
        private DateTime _censusDate;
        private AcademicYear _academicYear;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _testStartDate = new DateTime(2021,01,01);
            _censusDate = _testStartDate.AddDays(95);

            _mockApprenticeshipIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();
            _mockLearnerService = new Mock<ILearnerService>();
            _mockLogger = new Mock<ILogger<RefreshLearnerCommandHandler>>();

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            _incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .Create();

            _apprenticeshipIncentiveId = _incentiveModel.Id;
            _apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(_incentiveModel.Id, _incentiveModel);

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(_apprenticeshipIncentive);

            _learnerServiceResponse = _fixture.Create<LearnerServiceResponse>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(_learnerServiceResponse);

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(_apprenticeshipIncentive);

            _learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(_apprenticeshipIncentive))
                .ReturnsAsync(_learner);

            _collectionCalendarService = new Mock<ICollectionCalendarService>();
            _academicYear = new AcademicYear("2021", new DateTime(2021, 07, 31));

            _collectionPeriods = new List<CollectionCalendarPeriod>
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(9, 2021), 4, 2021, new DateTime(2021,4,1), _censusDate, true, false)
            };

            _collectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear> { _academicYear }, _collectionPeriods));

            _sut = new RefreshLearnerCommandHandler(
                _mockLogger.Object,
                _mockApprenticeshipIncentiveDomainRepository.Object,
                _mockLearnerService.Object,
                _mockLearnerDomainRepository.Object,
                _collectionCalendarService.Object);
        }

        [Test]
        public async Task Then_a_Learner_record_is_created_when_it_doesnt_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(It.IsAny<Learner>()), Times.Exactly(2));
        }

        [Test]
        public async Task Then_a_Learner_record_is_updated_when_it_already_exist()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(_learner), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_false_when_the_learner_data_does_not_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var learnerResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = null
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.SubmissionFound)
                ), Times.Once);
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_true_when_the_learner_data_does_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerServiceResponse = _fixture.Create<LearnerServiceResponse>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.SubmissionFound)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_learning_found_is_false_when_there_are_no_matching_training_entries_returned_from_the_learner_service()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerServiceResponse = _fixture.Create<LearnerServiceResponse>();

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.LearningData.LearningFound)
                ), Times.Exactly(2));
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
                                .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){
                                    _fixture.Build<PriceEpisodeDto>()
                                    .With(x => x.AcademicYear,"2021")
                                    .With(pe => pe.Periods, new List<PeriodDto>(){
                                        _fixture.Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, _apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                        })
                                    .With(pe => pe.StartDate, _testStartDate)
                                    .With(pe => pe.EndDate, _testStartDate.AddDays(2))
                                    .Create()
                                })
                                .Create(),
                            _fixture.Create<TrainingDto>()
                            })
                        .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            _censusDate = _testStartDate.AddDays(5);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningData.LearningFound)
                ), Times.Exactly(2));
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

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.SubmissionDate == testDate)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_startdate_is_set_when_one_exists_for_the_learner_data()
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
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                                                    .With(x => x.AcademicYear,"2021")
                                                    .With(pe => pe.Periods, new List<PeriodDto>(){
                                                        _fixture.Build<PeriodDto>()
                                                       .With(period => period.ApprenticeshipId, _incentiveModel.Apprenticeship.Id)
                                                       .With(period => period.IsPayable, true)
                                                       .Create()
                                                    })
                                                    .With(pe => pe.StartDate, _testStartDate)
                                                    .With(pe => pe.EndDate, _testStartDate.AddDays(2))
                                                    .Create() }
                        )
                        .Create(),
                    _fixture.Create<TrainingDto>() }
                    )
                .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            _censusDate = _testStartDate.AddDays(5);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningData.StartDate == _testStartDate)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_null_when_there_are_no_pending_payments()
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

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningData.IsInlearning == null)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_true_when_there_is_a_pending_payment_with_a_matching_inlearning_period()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            var pendingPaymentModel = _fixture.Create<PendingPaymentModel>();
            pendingPaymentModel.PaymentMadeDate = null;

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> { pendingPaymentModel })
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear, "2021")
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(-2))
                            .With(pe => pe.StartDate, pendingPaymentModel.DueDate.AddDays(-1))
                            .With(pe => pe.EndDate, pendingPaymentModel.DueDate.AddDays(1))
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.Period, pendingPaymentModel.CollectionPeriod.PeriodNumber)
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningData.IsInlearning == true)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_LearningPeriods_are_set_when_there_are_price_episodes_with_periods_for_the_apprenticeship()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear, "2021")
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.GetModel().LearningPeriods.Count == 1)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_learning_not_found_reason_is_logged() // EI-490
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();
            var learnerServiceResponse = new LearnerServiceResponse { LearnerSubmissionDto = learnerSubmissionDto };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLogger.VerifyLogContains(LogLevel.Information, Times.Once(), $"Start Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {_learner.ApprenticeshipIncentiveId}, ApprenticeshipId: {_learner.ApprenticeshipId}, UKPRN: {_learner.Ukprn}, ULN: {_learner.UniqueLearnerNumber}");
            _mockLogger.VerifyLogContains(LogLevel.Information, Times.Once(), $"End Learner data refresh from Learner match service for ApprenticeshipIncentiveId: {_learner.ApprenticeshipIncentiveId}, ApprenticeshipId: {_learner.ApprenticeshipId}, UKPRN: {_learner.Ukprn}, ULN: {_learner.UniqueLearnerNumber}");
            _mockLogger.VerifyLogContains(LogLevel.Information, Times.Once(), $"Matching ILR record not found for ApprenticeshipIncentiveId: {_learner.ApprenticeshipIncentiveId}, ApprenticeshipId: {_learner.ApprenticeshipId}, UKPRN: {_learner.Ukprn}, ULN: {_learner.UniqueLearnerNumber} with reason: {_learner.SubmissionData.LearningData.NotFoundReason}");
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_no_matching_learner_records()
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

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_true_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear,"2021")
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, _academicYear.EndDate.AddDays(-20))
                            .With(pe => pe.EndDate, _academicYear.EndDate.AddDays(-10))
                            .Create(),
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(x => x.AcademicYear,"2021")
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, _academicYear.EndDate.AddDays(-10))
                            .With(pe => pe.EndDate, _academicYear.EndDate.AddDays(-2))
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            _collectionPeriods = new List<CollectionCalendarPeriod>
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(12, 2021), 7, 2021, new DateTime(2021,7,1), new DateTime(2021, 7, 31), true, false)
            };

            _collectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear> { _academicYear }, _collectionPeriods));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                l.SubmissionData.LearningData.StoppedStatus.DateStopped == _academicYear.EndDate.AddDays(-1) &&
                !l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_not_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, DateTime.Today.AddDays(-20))
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(-10))
                            .Create(),
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, DateTime.Today.AddDays(-10))
                            .With(pe => pe.EndDate, DateTime.Today)
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                !l.SubmissionData.LearningData.StoppedStatus.DateStopped.HasValue &&
                l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue)
                ), Times.Exactly(2));
        }
       
        [Test]
        public async Task Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_resumed_from_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .With(p => p.Status, Enums.IncentiveStatus.Stopped)
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, DateTime.Today.AddDays(-20))
                            .With(pe => pe.EndDate, DateTime.Today.AddDays(10))
                            .Create(),
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.StartDate, DateTime.Today.AddDays(-10))
                            .With(pe => pe.EndDate, DateTime.Today)
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue &&
                !l.SubmissionData.LearningData.StoppedStatus.DateStopped.HasValue)
                ), Times.Exactly(2));
        }

        [Test]
        public async Task Then_null_price_episode_end_dates_are_set_to_the_end_of_the_academic_year()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();
            
            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .With(p => p.Status, Enums.IncentiveStatus.Active)
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
               .Build<LearnerSubmissionDto>()
               .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(t => t.PriceEpisodes,
                        new List<PriceEpisodeDto>{
                            _fixture
                            .Build<PriceEpisodeDto>()
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                        .Create()
                                    })
                            .With(pe => pe.AcademicYear, "2021")
                            .With(pe => pe.StartDate, DateTime.Today.AddDays(-20))
                            .With(pe => pe.EndDate, (DateTime?)null)
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()
                   })
               .Create();

            var learnerServiceResponse = new LearnerServiceResponse 
            {
                LearnerSubmissionDto = learnerSubmissionDto,
                RawJson = JsonConvert.SerializeObject(learnerSubmissionDto)
            };

            _mockLearnerService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerServiceResponse);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                It.Is<Learner>(l => l.GetModel().LearningPeriods.Single().EndDate == _academicYear.EndDate)
                ), Times.Exactly(2));
        }
    }
}

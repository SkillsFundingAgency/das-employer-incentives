﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.RefreshLearner;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.RefreshLearner.Handlers
{
    public class WhenHandlingRefreshLearnerCommand
    {
        private AcademicYear _academicYear;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _apprenticeshipIncentive;
        private Guid _apprenticeshipIncentiveId;
        private DateTime _censusDate;
        private Mock<ICollectionCalendarService> _collectionCalendarService;
        private List<CollectionCalendarPeriod> _collectionPeriods;
        private Fixture _fixture;
        private ApprenticeshipIncentiveModel _incentiveModel;
        private Learner _learner;
        private ILearnerService _learnerService;
        private LearnerSubmissionDto _learnerSubmissionDto;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockApprenticeshipIncentiveDomainRepository;
        private Mock<IDateTimeService> _mockDateTimeService;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;
        private Mock<ILearnerSubmissionService> _mockLearnerSubmissionService;
        private RefreshLearnerCommandHandler _sut;
        private DateTime _testStartDate;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));
            _testStartDate = new DateTime(2021, 01, 01);
            _censusDate = _testStartDate.AddDays(95);

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.Now()).Returns(DateTime.Now);
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);

            _mockApprenticeshipIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();
            _mockLearnerSubmissionService = new Mock<ILearnerSubmissionService>();

            var apprenticeship = _fixture.Create<Apprenticeship>();

            _incentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(x => x.HasPossibleChangeOfCircumstances, false)
                .With(p => p.RefreshedLearnerForEarnings, false)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            _apprenticeshipIncentiveId = _incentiveModel.Id;
            _apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(_incentiveModel.Id, _incentiveModel);

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(_apprenticeshipIncentive);

            _learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(_learnerSubmissionDto);

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(_apprenticeshipIncentive);

            _learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            _learner.GetModel().LearningPeriods.Clear();

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(_apprenticeshipIncentive))
                .ReturnsAsync(_learner);

            _collectionCalendarService = new Mock<ICollectionCalendarService>();
            _academicYear = new AcademicYear("2021", new DateTime(2021, 07, 31));

            _collectionPeriods = new List<CollectionCalendarPeriod>
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(9, 2021), 4, 2021, new DateTime(2021, 4, 1), _censusDate, true, false)
            };

            _collectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>
                    {
                        _academicYear
                    },
                    _collectionPeriods));

            _learnerService = new LearnerService(
                _mockLearnerSubmissionService.Object,
                _mockLearnerDomainRepository.Object,
                _collectionCalendarService.Object);

            _sut = new RefreshLearnerCommandHandler(
                _mockApprenticeshipIncentiveDomainRepository.Object,
                _learnerService,
                _mockDateTimeService.Object);
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

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(_learner), Times.Once);
        }

        [Test]
        public async Task Then_refreshed_learner_for_earnings_is_set()
        {
            // Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            // Act
            await _sut.Handle(command);

            // Assert
            _apprenticeshipIncentive.RefreshedLearnerForEarnings.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_false_when_the_learner_data_does_not_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(null as LearnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => !l.SubmissionData.SubmissionFound)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == false)));
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_true_when_the_learner_data_does_exist()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.SubmissionFound)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_the_learning_found_is_false_when_there_are_no_matching_training_entries_returned_from_the_learner_service()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => !l.SubmissionData.LearningData.LearningFound)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == false)));
        }

        [Test]
        public async Task Then_the_learning_found_is_true_when_there_are_matching_training_entries_returned_from_the_learner_service()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);
            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            _censusDate = _testStartDate.AddDays(5);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.LearningData.LearningFound)
                ),
                Times.Once);
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.SubmissionDate == testDate)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_the_startdate_is_set_when_one_exists_for_the_learner_data()
        {
            //Arrange            

            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(p => p.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture.Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
                                                _fixture.Build<PeriodDto>()
                                                    .With(period => period.ApprenticeshipId, _incentiveModel.Apprenticeship.Id)
                                                    .With(period => period.IsPayable, true)
                                                    .Create()
                                            })
                                        .With(pe => pe.StartDate, _testStartDate)
                                        .With(pe => pe.EndDate, _testStartDate.AddDays(2))
                                        .Create()
                                }
                            )
                            .Create(),
                        _fixture.Create<TrainingDto>()
                    }
                )
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            _censusDate = _testStartDate.AddDays(5);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.LearningData.StartDate == _testStartDate)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == true)));
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_null_when_there_are_no_pending_payments()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .Create(),
                        _fixture.Create<TrainingDto>()
                    })
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.LearningData.IsInlearning == null)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == false)));
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_true_when_there_is_a_pending_payment_with_a_matching_inlearning_period()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var pendingPaymentModel = _fixture.Create<PendingPaymentModel>();
            pendingPaymentModel.PaymentMadeDate = null;
            pendingPaymentModel.CollectionPeriod = new Domain.ValueObjects.CollectionPeriod(5, 2021);

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.PendingPaymentModels,
                    new List<PendingPaymentModel>
                    {
                        pendingPaymentModel
                    })
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, pendingPaymentModel.CollectionPeriod.AcademicYear.ToString())
                                        .With(pe => pe.EndDate, DateTime.Today.AddDays(-2))
                                        .With(pe => pe.StartDate, pendingPaymentModel.DueDate.AddDays(-1))
                                        .With(pe => pe.EndDate, pendingPaymentModel.DueDate.AddDays(1))
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.LearningData.IsInlearning == true)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_the_LearningPeriods_are_set_when_there_are_price_episodes_with_periods_for_the_apprenticeship()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(pe => pe.StartDate, new DateTime(2020, 10, 1))
                                        .Without(pe => pe.EndDate)
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.GetModel().LearningPeriods.Count == 1)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_no_matching_learner_records()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .Create(),
                        _fixture.Create<TrainingDto>()
                    })
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_true_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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
                                        .With(x => x.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            _collectionPeriods = new List<CollectionCalendarPeriod>
            {
                new CollectionCalendarPeriod(new Domain.ValueObjects.CollectionPeriod(12, 2021),
                    7,
                    2021,
                    new DateTime(2021, 7, 1),
                    new DateTime(2021, 7, 31),
                    true,
                    false)
            };

            _collectionCalendarService
                .Setup(m => m.Get())
                .ReturnsAsync(new Domain.ValueObjects.CollectionCalendar(new List<AcademicYear>
                    {
                        _academicYear
                    },
                    _collectionPeriods));

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                                        l.SubmissionData.LearningData.StoppedStatus.DateStopped == _academicYear.EndDate.AddDays(-1) &&
                                        !l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == true)));
        }

        [Test]
        public async Task Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_not_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                                        !l.SubmissionData.LearningData.StoppedStatus.DateStopped.HasValue &&
                                        l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue)
                ),
                Times.Once);
        }

        [Test]
        public async Task
            Then_the_learning_stopped_status_has_false_stopped_flag_when_there_are_matching_learner_records_and_the_learning_has_resumed_from_stopped()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Status, IncentiveStatus.Stopped)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Without(p => p.PendingPaymentModels)
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => !l.SubmissionData.LearningData.StoppedStatus.LearningStopped &&
                                        l.SubmissionData.LearningData.StoppedStatus.DateResumed.HasValue &&
                                        !l.SubmissionData.LearningData.StoppedStatus.DateStopped.HasValue)
                ),
                Times.Once);

            _mockApprenticeshipIncentiveDomainRepository.Verify(x
                => x.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.HasPossibleChangeOfCircumstances == true)));
        }

        [Test]
        public async Task Then_null_price_episode_end_dates_are_set_to_the_end_of_the_academic_year()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Without(p => p.PendingPaymentModels)
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            //Fixed date for testing purposes
            var fixedDate = new DateTime(2023, 06, 06);
            var startDate = fixedDate.AddMonths(-30);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
                                                _fixture
                                                    .Build<PeriodDto>()
                                                    .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                                    .Create()
                                            })
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.StartDate, startDate)
                                        .With(pe => pe.EndDate, (DateTime?)null)
                                        .Create()
                                }).Create(),
                        _fixture.Create<TrainingDto>()
                    })
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.GetModel().LearningPeriods.Single().EndDate == _academicYear.EndDate)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_HasPossibleChangeOfCircumstances_flag_is_set_to_True_when_periods_have_changed()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Without(p => p.PendingPaymentModels)
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            //Fixed date for testing purposes
            var fixedDate = new DateTime(2023, 06, 06);
            var startDate = fixedDate.AddMonths(-30);

            var learningData = new LearningData(true);
            learningData.SetStartDate(startDate);
            var submissionData = new SubmissionData();
            submissionData.SetLearningData(learningData);
            var learner = new LearnerFactory().GetExisting(_fixture.Build<LearnerModel>()
                .With(x => x.SubmissionData, submissionData)
                .Without(x => x.LearningPeriods)
                .Create());


            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
                                                _fixture
                                                    .Build<PeriodDto>()
                                                    .With(p => p.ApprenticeshipId, apprenticeshipIncentive.Apprenticeship.Id)
                                                    .Create()
                                            })
                                        .With(pe => pe.AcademicYear, "2021")
                                        .With(pe => pe.StartDate, startDate)
                                        .With(pe => pe.EndDate, (DateTime?)null)
                                        .Create()
                                }).Create(),
                        _fixture.Create<TrainingDto>()
                    })
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.HasPossibleChangeOfCircumstances == true)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_HasPossibleChangeOfCircumstances_flag_is_set_to_False_when_periods_have_not_changed()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Without(p => p.PendingPaymentModels)
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var episode1 = new PriceEpisodeDto
            {
                AcademicYear = "2021",
                StartDate = new DateTime(2021, 4, 1),
                EndDate = new DateTime(2021, 6, 30),
                Periods = new[]
                {
                    new PeriodDto
                    {
                        ApprenticeshipId = apprenticeship.Id
                    }
                }
            };

            var episode2 = new PriceEpisodeDto
            {
                AcademicYear = "2021",
                StartDate = new DateTime(2021, 9, 1),
                EndDate = new DateTime(2021, 12, 30),
                Periods = new[]
                {
                    new PeriodDto
                    {
                        ApprenticeshipId = apprenticeship.Id
                    }
                }
            };

            var periods = new List<LearningPeriod> // change order because it cannot be guaranteed when stored
            {
                new LearningPeriod(episode2.StartDate, episode2.EndDate.Value),
                new LearningPeriod(episode1.StartDate, episode1.EndDate.Value)
            };

            var learningData = new LearningData(true);
            learningData.SetStartDate(episode1.StartDate);
            var submissionData = new SubmissionData();
            submissionData.SetLearningData(learningData);
            var learner = new LearnerFactory().GetExisting(_fixture.Build<LearnerModel>()
                .With(x => x.SubmissionData, submissionData)
                .With(model => model.LearningPeriods, periods)
                .Create());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    episode1,
                                    episode2
                                }).Create(),
                        _fixture.Create<TrainingDto>()
                    })
                .Create();

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockLearnerDomainRepository.Verify(m => m.Save(
                    It.Is<Learner>(l => l.HasPossibleChangeOfCircumstances == false)
                ),
                Times.Once);
        }

        [Test]
        public async Task Then_employment_checks_are_created_when_in_learning()
        {
            //Arrange
            var command = new RefreshLearnerCommand(_apprenticeshipIncentiveId);

            var apprenticeship = _fixture.Create<Apprenticeship>();

            var pendingPaymentModel = _fixture.Create<PendingPaymentModel>();
            pendingPaymentModel.PaymentMadeDate = null;
            pendingPaymentModel.CollectionPeriod = new Domain.ValueObjects.CollectionPeriod(5, 2021);
            pendingPaymentModel.DueDate = new DateTime(2021, 09, 01);

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.StartDate, new DateTime(2021, 06, 01))
                .With(p => p.Apprenticeship, apprenticeship)
                .With(p => p.Status, IncentiveStatus.Active)
                .With(p => p.PreviousStatus, IncentiveStatus.Active)
                .With(p => p.PendingPaymentModels,
                    new List<PendingPaymentModel>
                    {
                        pendingPaymentModel
                    })
                .With(p => p.Phase, new IncentivePhase(Phase.Phase2))
                .Without(p => p.EmploymentCheckModels)
                .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_apprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                    apprenticeshipIncentiveModel));

            var apprenticeshipIncentive = new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.Id, apprenticeshipIncentiveModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _mockLearnerDomainRepository
                .Setup(m => m.GetOrCreate(apprenticeshipIncentive))
                .ReturnsAsync(learner);

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.AcademicYear, "2021")
                .With(l => l.Training,
                    new List<TrainingDto>
                    {
                        _fixture.Create<TrainingDto>(),
                        _fixture
                            .Build<TrainingDto>()
                            .With(p => p.Reference, "ZPROG001")
                            .With(t => t.PriceEpisodes,
                                new List<PriceEpisodeDto>
                                {
                                    _fixture
                                        .Build<PriceEpisodeDto>()
                                        .With(x => x.AcademicYear, pendingPaymentModel.CollectionPeriod.AcademicYear.ToString())
                                        .With(pe => pe.EndDate, DateTime.Today.AddDays(-2))
                                        .With(pe => pe.StartDate, pendingPaymentModel.DueDate.AddDays(-1))
                                        .With(pe => pe.EndDate, pendingPaymentModel.DueDate.AddDays(1))
                                        .With(pe => pe.Periods,
                                            new List<PeriodDto>
                                            {
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

            _mockLearnerSubmissionService
                .Setup(m => m.Get(It.IsAny<Learner>()))
                .ReturnsAsync(learnerSubmissionDto);

            //Act
            await _sut.Handle(command);

            //Assert
            _mockApprenticeshipIncentiveDomainRepository.Verify(m => m.Save(
                    It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(l => l.EmploymentChecks.Count == 2)
                ),
                Times.Once);
        }
    }
}
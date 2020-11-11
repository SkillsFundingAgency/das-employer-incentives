﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenRefreshCalled
    {
        private Commands.Services.LearnerMatchApi.LearnerService _sut;
        private Mock<Commands.Persistence.IApprenticeshipIncentiveDomainRepository> _mockApprenticeshipIncentiveDomainRepository;
        private TestHttpClient _httpClient;
        private Uri _baseAddress;
        private Learner _learner;
        private readonly string _version = "1.0";
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _baseAddress = new Uri(@"http://localhost");
            _httpClient = new TestHttpClient(_baseAddress);

            _mockApprenticeshipIncentiveDomainRepository = new Mock<Commands.Persistence.IApprenticeshipIncentiveDomainRepository>();

            var apprenticeship = _fixture.Create<Apprenticeship>();
            apprenticeship.SetProvider(_fixture.Create<Provider>());

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, apprenticeship)
                .Create();

            _learner = new Learner(
                _fixture.Create<Guid>(),
                apprenticeshipIncentiveModel.ApplicationApprenticeshipId,
                apprenticeshipIncentiveModel.Apprenticeship.Id,
                apprenticeshipIncentiveModel.Apprenticeship.Provider.Ukprn,
                apprenticeshipIncentiveModel.Apprenticeship.UniqueLearnerNumber,
                _fixture.Create<DateTime>());

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_learner.ApprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

            _sut = new Commands.Services.LearnerMatchApi.LearnerService(_httpClient, _version, _mockApprenticeshipIncentiveDomainRepository.Object);
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_false_when_the_learner_data_does_not_exist()
        {
            //Arrange
            _httpClient.SetUpGetAsAsync(System.Net.HttpStatusCode.NotFound);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _httpClient.VerifyGetAsAsync($"api/v{_version}/{_learner.Ukprn}/{_learner.UniqueLearnerNumber}?", Times.Once());
            _learner.SubmissionFound.Should().BeFalse();
        }

        [Test]
        public async Task Then_the_learner_submissionfound_is_true_when_the_learner_data_does_not_exist()
        {
            //Arrange
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _httpClient.VerifyGetAsAsync($"api/v{_version}/{_learner.Ukprn}/{_learner.UniqueLearnerNumber}?", Times.Once());
            _learner.SubmissionFound.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_learning_found_is_false_when_there_are_no_matching_training_entries_returned_from_the_matching_service()
        {
            //Arrange
            var learnerSubmissionDto = _fixture.Create<LearnerSubmissionDto>();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.LearningFound.Should().BeFalse();
        }

        [Test]
        public async Task Then_the_learning_found_is_true_when_there_are_matching_training_entries_returned_from_the_matching_service()
        {
            //Arrange
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

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.LearningFound.Should().BeTrue();
        }

        [Test]
        public async Task Then_the_submission_date_is_true_when_the_learner_data_exists()
        {
            //Arrange
            var testDate = DateTime.Now;

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(p => p.IlrSubmissionDate, testDate)                
                .Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.SubmissionDate.Should().Be(testDate);
        }

        [Test]
        public async Task Then_the_startdate_is_set_when_one_exists_for_the_learner_data()
        {
            //Arrange           
            var _testStartDate = _fixture.Create<DateTime>();

            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                                                    .With(pe => pe.Periods, new List<PeriodDto>(){
                                                        _fixture.Build<PeriodDto>()
                                                       .With(period => period.ApprenticeshipId, _learner.ApprenticeshipId)
                                                       .With(period => period.IsPayable, true)
                                                       .Create()
                                                    })
                                                    .With(pe => pe.StartDate, _testStartDate)
                                                    .Create() }
                        )
                        .Create(),
                    _fixture.Create<TrainingDto>() }
                    )
                .Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.StartDate.Should().Be(_testStartDate);
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_false_when_there_are_no_pending_payments()
        {
            //Arrange
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

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.IsInlearning.Should().BeFalse();
        }

        [Test]
        public async Task Then_the_inlearning_flag_is_true_when_there_is_a_pending_payment_with_a_matching_inlearning_period()
        {
            //Arrange
            var apprenticeship = _fixture.Create<Apprenticeship>();
            apprenticeship.SetProvider(_fixture.Create<Provider>());

            var pendingPaymentModel = _fixture.Create<PendingPaymentModel>();
            pendingPaymentModel.PaymentMadeDate = null;

            var apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
               .With(p => p.Apprenticeship, apprenticeship)
               .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> { pendingPaymentModel })
               .Create();

            _mockApprenticeshipIncentiveDomainRepository
                .Setup(m => m.Find(_learner.ApprenticeshipIncentiveId))
                .ReturnsAsync(new ApprenticeshipIncentiveFactory().GetExisting(apprenticeshipIncentiveModel.ApplicationApprenticeshipId, apprenticeshipIncentiveModel));

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
                            .With(pe => pe.StartDate, pendingPaymentModel.DueDate.AddDays(-1))
                            .With(pe => pe.EndDate, pendingPaymentModel.DueDate.AddDays(1))
                            .With(pe => pe.Periods,
                                    new List<PeriodDto>{
                                        _fixture
                                        .Build<PeriodDto>()
                                        .With(p => p.Period, pendingPaymentModel.PeriodNumber)
                                        .With(p => p.ApprenticeshipId, _learner.ApprenticeshipId)
                                        .Create()
                                    })
                            .Create()
                            }).Create(),
                        _fixture.Create<TrainingDto>()                    
                    })
                .Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);            

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.SubmissionData.IsInlearning.Should().BeTrue();
        }
    }
}

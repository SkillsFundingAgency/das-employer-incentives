﻿using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Commands.ApprenticeshipIncentive.LearnerChangeOfCircumstance;
using SFA.DAS.EmployerIncentives.Commands.Persistence;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.ApprenticeshipIncentive.LearnerChangeOfCircumstance
{
    public class WhenHandlingLearnerChangeOfCircumstanceCommand
    {
        private LearnerChangeOfCircumstanceCommandHandler _sut;
        private Mock<IApprenticeshipIncentiveDomainRepository> _mockIncentiveDomainRepository;
        private Fixture _fixture;
        private Mock<ILearnerDomainRepository> _mockLearnerDomainRepository;        
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _incentiveModel;
        private Learner _learner;
        private Mock<ICollectionCalendarService> _mockCollectionCalendarService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _mockIncentiveDomainRepository = new Mock<IApprenticeshipIncentiveDomainRepository>();
            _mockLearnerDomainRepository = new Mock<ILearnerDomainRepository>();

            var collectionPeriods = new List<Domain.ValueObjects.CollectionPeriod>()
            {
                new Domain.ValueObjects.CollectionPeriod(1, _fixture.Create<byte>(), _fixture.Create<short>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<short>(), true, false),
            };
            var collectionCalendar = new Domain.ValueObjects.CollectionCalendar(collectionPeriods);

            _mockCollectionCalendarService = new Mock<ICollectionCalendarService>();
            _mockCollectionCalendarService.Setup(m => m.Get()).ReturnsAsync(collectionCalendar);

            _incentiveModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, 
                        new Apprenticeship(
                            _fixture.Create<long>(),
                            _fixture.Create<string>(),
                            _fixture.Create<string>(),
                            DateTime.Today.AddYears(-26),
                            _fixture.Create<long>(),
                            ApprenticeshipEmployerType.Levy,
                            _fixture.Create<string>(),
                            _fixture.Create<DateTime>()
                        ))
                .With(p => p.StartDate, DateTime.Today)
                .With(p => p.Status, Enums.IncentiveStatus.Active)
                .With(p => p.HasPossibleChangeOfCircumstances, true)
                .With(p => p.Phase, new IncentivePhase(Phase.Phase1))
                .With(p => p.MinimumAgreementVersion, new AgreementVersion(_fixture.Create<int>()))
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel>())
                .Create();
            _incentiveModel.Apprenticeship.SetProvider(_fixture.Create<Provider>());
            _incentiveModel.HasPossibleChangeOfCircumstances = true;
            var incentive = new ApprenticeshipIncentiveFactory().GetExisting(_incentiveModel.Id, _incentiveModel);
            _fixture.Register(() => incentive);

            _sut = new LearnerChangeOfCircumstanceCommandHandler(_mockIncentiveDomainRepository.Object, _mockLearnerDomainRepository.Object, _mockCollectionCalendarService.Object);

            _incentive = _fixture.Create<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>();
            _learner = new LearnerFactory().GetExisting(
                _fixture.Build<LearnerModel>()
                .With(x => x.SubmissionData, _fixture.Create<SubmissionData>())
                .With(x=> x.ApprenticeshipIncentiveId, _incentive.Id)
                .Create());

            _mockIncentiveDomainRepository.Setup(x => x.Find(incentive.Id)).ReturnsAsync(_incentive);
            _mockLearnerDomainRepository.Setup(m => m.GetOrCreate(incentive)).ReturnsAsync(_learner);
        }

        [Test]
        public async Task Then_the_start_date_is_updated()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(_learner.SubmissionData.LearningData.StartDate.Value);
        }

        [Test]
        public async Task Then_the_StartDateChanged_event_is_raised()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            _learner.SubmissionData.SetLearningData(new LearningData(true));
            var newStartDate = _incentive.StartDate.AddMonths(2);
            var previousStartDate = _incentive.StartDate;
            _learner.SubmissionData.LearningData.SetStartDate(newStartDate);

            // Act
            await _sut.Handle(command);

            // Assert
            var @event = _incentive.FlushEvents().Single(e => e is StartDateChanged) as StartDateChanged;
            @event.ApprenticeshipIncentiveId.Should().Be(_incentive.Id);
            @event.NewStartDate.Should().Be(newStartDate);
            @event.PreviousStartDate.Should().Be(previousStartDate);
        }

        [Test]
        public async Task Then_the_StartDateChanged_event_is_not_raised_if_the_start_date_is_the_same_as_the_previous_start_date()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.SetSubmissionDate(_fixture.Create<DateTime>());
            _learner.SubmissionData.SetLearningData(new LearningData(true));
            _learner.SubmissionData.LearningData.SetStartDate(_incentive.StartDate);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.FlushEvents().Count(e => e is StartDateChanged).Should().Be(0);
        }

        [Test]
        public async Task Then_the_changes_are_persisted()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.LearningData.SetStartDate(_fixture.Create<DateTime>());

            int itemsPersisted = 0;
            _mockIncentiveDomainRepository.Setup(m => m.Save(It.Is<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>(a => a.Id == command.ApprenticeshipIncentiveId)))
                .Callback(() =>
                {
                    itemsPersisted++;
                });


            // Act
            await _sut.Handle(command);

            // Assert
            itemsPersisted.Should().Be(1);
        }

        [Test]
        public async Task Then_the_start_date_is_not_updated_when_submission_data_was_not_found()
        {
            //Arrange
            var originalStartDate = _incentive.StartDate;
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SetSubmissionData(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(originalStartDate);
        }

        [Test]
        public async Task Then_the_start_date_is_not_updated_when_submission_has_no_start_date()
        {
            //Arrange
            var originalStartDate = _incentive.StartDate;
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SubmissionData.LearningData.SetStartDate(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.StartDate.Should().Be(originalStartDate);
        }

        [Test]
        public async Task Then_HasPossibleChangeOfCircumstances_set_to_false_when_change_of_circs_complete()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.HasPossibleChangeOfCircumstances.Should().BeFalse();
        }

        [Test]
        public async Task Then_process_should_exit_if_no_possible_change_of_circumstances()
        {
            //Arrange
            _incentive.SetHasPossibleChangeOfCircumstances(false);
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);

            // Act
            await _sut.Handle(command);

            // Assert
            _mockLearnerDomainRepository.Verify(x => x.GetOrCreate(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
            _mockIncentiveDomainRepository.Verify(x => x.Save(It.IsAny<Domain.ApprenticeshipIncentives.ApprenticeshipIncentive>()), Times.Never);
        }

        [Test]
        public async Task Then_the_learning_stopped_status_is_not_updated_when_submission_data_was_not_found()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SetSubmissionData(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.GetModel().Status.Should().Be(IncentiveStatus.Active);
        }

        [Test]
        public async Task Then_the_learning_stopped_event_is_not_raise_when_submission_data_was_not_found()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            _learner.SetSubmissionData(null);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.FlushEvents().Count(e => e is LearningStopped).Should().Be(0);
        }

        [Test]
        public async Task Then_the_learning_stopped_event_is_raised_when_learning_is_stopped()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            var stoppedStatus = new LearningStoppedStatus(true, DateTime.Now.AddDays(-2));

            var learningData = new LearningData(true);
            learningData.SetIsStopped(stoppedStatus);
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetSubmissionDate(DateTime.Today);
            submissionData.SetLearningData(learningData);

            _learner = new LearnerFactory().GetExisting(
               _fixture.Build<LearnerModel>()
               .With(x => x.SubmissionData, submissionData)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .Create());

            _mockLearnerDomainRepository.Setup(m => m.GetOrCreate(_incentive)).ReturnsAsync(_learner);

            // Act
            await _sut.Handle(command);

            // Assert
            var @event = _incentive.FlushEvents().Single(e => e is LearningStopped) as LearningStopped;
            @event.ApprenticeshipIncentiveId.Should().Be(_incentive.Id);
            @event.StoppedDate.Should().Be(stoppedStatus.DateStopped.Value);
        }

        [Test]
        public async Task Then_the_learning_stopped_status_is_set_when_learning_is_stopped()
        {
            //Arrange
            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);
            var stoppedStatus = new LearningStoppedStatus(true, DateTime.Now.AddDays(-2));

            var learningData = new LearningData(true);
            learningData.SetIsStopped(stoppedStatus);
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetSubmissionDate(DateTime.Today);
            submissionData.SetLearningData(learningData);

            _learner = new LearnerFactory().GetExisting(
               _fixture.Build<LearnerModel>()
               .With(x => x.SubmissionData, submissionData)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .Create());

            _mockLearnerDomainRepository.Setup(m => m.GetOrCreate(_incentive)).ReturnsAsync(_learner);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.GetModel().Status.Should().Be(IncentiveStatus.Stopped);
        }

        [Test]
        public async Task Then_the_learning_stopped_event_is_raised_when_learning_is_stopped_and_the_incentive_is_already_in_a_stopped_state()
        {
            //Arrange
            _incentiveModel.Status = IncentiveStatus.Stopped;

            var command = new LearnerChangeOfCircumstanceCommand(_incentive.Id);

            var stoppedStatus = new LearningStoppedStatus(true, DateTime.Now.AddDays(-2));

            var learningData = new LearningData(true);
            learningData.SetIsStopped(stoppedStatus);
            var submissionData = _fixture.Create<SubmissionData>();
            submissionData.SetSubmissionDate(DateTime.Today);
            submissionData.SetLearningData(learningData);

            _learner = new LearnerFactory().GetExisting(
               _fixture.Build<LearnerModel>()
               .With(x => x.SubmissionData, submissionData)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .With(x => x.ApprenticeshipIncentiveId, _incentive.Id)
               .Create());

            _mockLearnerDomainRepository.Setup(m => m.GetOrCreate(_incentive)).ReturnsAsync(_learner);

            // Act
            await _sut.Handle(command);

            // Assert
            _incentive.FlushEvents().Count(e => e is LearningStopped).Should().Be(0);
        }
    }
}

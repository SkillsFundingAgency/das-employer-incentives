using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenLearningFoundForApprenticeshipCalculated
    {
        private DateTime _startDate;
        private Domain.ApprenticeshipIncentives.ApprenticeshipIncentive _incentive;
        private ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _startDate = DateTime.Now;
            var dueDate = _startDate.AddMonths(1);

            var pendingPaymentModel = _fixture
                .Build<PendingPaymentModel>()
                .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                .With(p => p.DueDate, dueDate)
                .Create();
            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(p => p.Apprenticeship, _fixture.Create<Apprenticeship>())
                .With(p => p.PendingPaymentModels, new List<PendingPaymentModel> {
                     _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(1))
                    .With(pp => pp.CollectionPeriod, (Domain.ValueObjects.CollectionPeriod)null)
                    .Create(),
                    _fixture.Build<PendingPaymentModel>()
                    .With(pp => pp.PaymentMadeDate, (DateTime?)null)
                    .With(pp => pp.DueDate, dueDate.AddMonths(2))
                    .With(pp => pp.CollectionPeriod, (Domain.ValueObjects.CollectionPeriod)null)
                    .Create(),
                    pendingPaymentModel
                    })
                .Create();

            _incentive = new ApprenticeshipIncentiveFactory().GetExisting(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
        }

        [Test]
        public void Then_the_learning_found_is_false_when_there_are_no_matching_training_entries_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture.Create<LearnerSubmissionDto>();
            sut.Training.Clear();

            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeFalse();
            status.NotFoundReason.Should().Be("No learning aims were found");
        }

        [Test]
        public void Then_the_learning_found_is_false_when_there_training_is_null_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture.Create<LearnerSubmissionDto>();
            sut.Training = null;

            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeFalse();
            status.NotFoundReason.Should().Be("No learning aims were found");
        }

        [Test]
        public void Then_the_learning_found_is_false_when_there_are_no_ZPROG001_training_entries_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture.Create<LearnerSubmissionDto>();
            foreach (var training in sut.Training)
            {
                training.Reference = "x";
            }

            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeFalse();
            status.NotFoundReason.Should().Be("Learning aims were found for the ULN, but none of them had a reference of ZPROG001");
        }

        [Test]
        public void Then_the_learning_found_is_false_when_there_are_no_matching_price_episodes_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture.Create<LearnerSubmissionDto>();
            sut.Training.First().Reference = "ZPROG001";
            sut.Training.First().PriceEpisodes.Clear();


            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeFalse();
            status.NotFoundReason.Should().Be("A ZPROG001 aim was found, but it had no price episodes");
        }

        [Test]
        public void Then_the_learning_found_is_false_when_there_are_no_matching_price_episodes_periods_with_the_apprenticeship_id_that_matches_the_commitment_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture.Create<LearnerSubmissionDto>();
            sut.Training.First().Reference = "ZPROG001";
            foreach (var period in sut.Training.First().PriceEpisodes.First().Periods)
            {
                period.ApprenticeshipId = _incentive.Apprenticeship.Id - 1; // not matching !
            }

            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeFalse();
            status.NotFoundReason.Should().Be("A ZPROG001 aim was found, with price episodes, but with no periods with the apprenticeship id that matches the commitment");
        }

        [Test]
        public void Then_the_learning_found_is_true_when_there_are_matching_training_entries_returned_from_the_matching_service()
        {
            //Arrange
            var sut = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>()
                        {
                            new PriceEpisodeDto { Periods = new List<PeriodDto>() { new PeriodDto()
                            {
                                ApprenticeshipId = _incentive.Apprenticeship.Id
                            } }}
                        })
                        .Create(),
                    _fixture.Create<TrainingDto>()
                    })
                .Create();

            //Act
            var status = sut.LearningFound(_incentive);

            //Assert
            status.LearningFound.Should().BeTrue();
        }
    }
}

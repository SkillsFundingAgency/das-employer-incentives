using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenRequestingEmploymentChecks
    {
        private Fixture _fixture;
        private ApprenticeshipIncentiveModel _sutModel;
        private ApprenticeshipIncentive _sut;
        private Mock<IDateTimeService> _mockDateTimeService;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));

            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(m => m.UtcNow()).Returns(DateTime.UtcNow);

            var startDate = DateTime.Now.AddDays(-42);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>()
                .With(x => x.StartDate, startDate)
                .With(x => x.Status, IncentiveStatus.Active)
                .Without(x => x.EmploymentCheckModels)
                .Create();

            _sut = Sut(_sutModel);
        }

        [TestCase("2020-09-01", "2020-07-31", Phase.Phase1)]
        [TestCase("2021-06-01", "2021-03-31", Phase.Phase2)]
        public void Then_employment_checks_are_added_when_none_exist(DateTime startDate, DateTime expectedFirstCheckEndDate, Phase phase)
        {
            var incentivePhase = new IncentivePhase(phase);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.StartDate, startDate).With(x => x.Phase, incentivePhase).Without(x => x.EmploymentCheckModels).Create();
            _sut = Sut(_sutModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            learner.SubmissionData.SetLearningData(new LearningData(true));
            _sut.RefreshLearner(learner, _mockDateTimeService.Object);

            _sut.EmploymentChecks.Count.Should().Be(2);

            var firstCheck = _sut.EmploymentChecks.Single(x => x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);
            firstCheck.MaximumDate.Should().Be(expectedFirstCheckEndDate);
            firstCheck.MinimumDate.Should().Be(expectedFirstCheckEndDate.AddMonths(-6).AddDays(1));

            var secondCheck = _sut.EmploymentChecks.Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);
            secondCheck.MinimumDate.Should().Be(startDate);
            secondCheck.MaximumDate.Should().Be(startDate.AddDays(42));

            var events = _sut.FlushEvents().OfType<EmploymentChecksCreated>();
        
            events.Count().Should().Be(2);

            @events.First().ApprenticeshipIncentiveId.Should().Be(_sutModel.Id);
            @events.First().Model.CheckType.Should().Be(EmploymentCheckType.EmployedBeforeSchemeStarted);
            @events.Last().ApprenticeshipIncentiveId.Should().Be(_sutModel.Id);
            @events.Last().Model.CheckType.Should().Be(EmploymentCheckType.EmployedAtStartOfApprenticeship);
        }

        [Test]
        public void Then_employment_checks_are_not_added_when_checks_already_exist()
        {
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().Create();

            _sut = Sut(_sutModel);
            var expectedEmploymentChecks = _sut.EmploymentChecks;

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            learner.SubmissionData.SetLearningData(new LearningData(true));
            _sut.RefreshLearner(learner, _mockDateTimeService.Object);

            _ = _sut.EmploymentChecks.Count.Should().Be(expectedEmploymentChecks.Count);
        }

        [Test]
        public void Then_employment_checks_are_not_added_when_learning_not_found()
        {
            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            learner.SubmissionData.SetLearningData(new LearningData(false));
            _sut.RefreshLearner(learner, _mockDateTimeService.Object);

            _sut.EmploymentChecks.Count.Should().Be(0);
        }

        [Test]
        public void Then_employment_checks_are_not_added_when_start_date_was_less_than_six_weeks_ago()
        {
            var startDate = DateTime.Now.AddDays(-41);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.StartDate, startDate).Without(x => x.EmploymentCheckModels).Create();

            _sut = Sut(_sutModel);

            var learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());
            learner.SubmissionData.SetLearningData(new LearningData(true));
            _sut.RefreshLearner(learner, _mockDateTimeService.Object);

            _sut.EmploymentChecks.Count.Should().Be(0);
        }
        
        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

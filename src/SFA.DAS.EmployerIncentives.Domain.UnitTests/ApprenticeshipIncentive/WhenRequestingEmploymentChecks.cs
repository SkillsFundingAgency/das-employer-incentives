using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenRequestingEmploymentChecks
    {
        private Fixture _fixture;
        private ApprenticeshipIncentiveModel _sutModel;
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var startDate = DateTime.Now.AddDays(-42);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.StartDate, startDate).Without(x => x.EmploymentCheckModels).Create();

            _sut = Sut(_sutModel);
        }

        [TestCase("2020-09-01", "2020-07-31", Phase.Phase1)]
        [TestCase("2021-06-01", "2021-03-31", Phase.Phase2)]
        public void Then_employment_checks_are_added_when_none_exist(DateTime startDate, DateTime expectedFirstCheckEndDate, Phase phase)
        {
            var incentivePhase = new IncentivePhase(phase);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.StartDate, startDate).With(x => x.Phase, incentivePhase).Without(x => x.EmploymentCheckModels).Create();
            _sut = Sut(_sutModel);

            _sut.RequestEmploymentChecks(true);

            _sut.EmploymentChecks.Count.Should().Be(2);

            var firstCheck = _sut.EmploymentChecks.Single(x => x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);
            firstCheck.MaximumDate.Should().Be(expectedFirstCheckEndDate);
            firstCheck.MinimumDate.Should().Be(expectedFirstCheckEndDate.AddMonths(-6).AddDays(1));

            var secondCheck = _sut.EmploymentChecks.Single(x => x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);
            secondCheck.MinimumDate.Should().Be(startDate);
            secondCheck.MaximumDate.Should().Be(startDate.AddDays(42));

            var events = _sut.FlushEvents();

            var expectedEvent = events.Single() as EmploymentChecksCreated;
            expectedEvent.ApprenticeshipIncentiveId.Should().Be(_sutModel.Id);
        }

        [Test]
        public void Then_employment_checks_are_not_added_when_checks_already_exist()
        {
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().Create();

            _sut = Sut(_sutModel);
            var expectedEmploymentChecks = _sut.EmploymentChecks;

            _sut.RequestEmploymentChecks(true);

            _sut.EmploymentChecks.Count().Should().Be(expectedEmploymentChecks.Count);
        }

        [TestCase(false)]
        [TestCase(null)]
        public void Then_employment_checks_are_not_added_when_not_in_learning(bool inLearning)
        {
            _sut.RequestEmploymentChecks(inLearning);

            _sut.EmploymentChecks.Count.Should().Be(0);
        }

        [Test]
        public void Then_employment_checks_are_not_added_when_start_date_was_less_than_six_weeks_ago()
        {
            var startDate = DateTime.Now.AddDays(-41);
            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.StartDate, startDate).Without(x => x.EmploymentCheckModels).Create();

            _sut = Sut(_sutModel);

            _sut.RequestEmploymentChecks(true);

            _sut.EmploymentChecks.Count.Should().Be(0);
        }
        
        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

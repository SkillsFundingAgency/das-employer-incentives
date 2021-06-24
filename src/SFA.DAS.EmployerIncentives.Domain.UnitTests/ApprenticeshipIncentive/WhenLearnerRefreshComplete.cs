using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenLearnerRefreshComplete
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.RefreshedLearnerForEarnings, false).Create();
            
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_RefreshedLearnerForEarnings_is_true()
        {
            _sut.LearnerRefreshCompleted();

            _sut.RefreshedLearnerForEarnings.Should().BeTrue();
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

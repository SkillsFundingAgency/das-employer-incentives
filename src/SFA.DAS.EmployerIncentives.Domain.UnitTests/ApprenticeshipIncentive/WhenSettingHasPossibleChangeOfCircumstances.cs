using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenSettingHasPossibleChangeOfCircumstances
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture.Build<ApprenticeshipIncentiveModel>().With(p => p.HasPossibleChangeOfCircumstances, false).Create();
            
            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_the_field_is_updated()
        {
            _sut.SetHasPossibleChangeOfCircumstances(true);

            _sutModel.HasPossibleChangeOfCircumstances.Should().BeTrue();
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

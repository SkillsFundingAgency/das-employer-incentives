using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenUpdateEmploymentCheck
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Without(i => i.EmploymentCheckModels)
                .Create();            
        }

        [TestCase(EmploymentCheckType.EmployedAtStartOfApprenticeship)]
        [TestCase(EmploymentCheckType.EmployedBeforeSchemeStarted)]
        public void Then_a_superseded_employment_check_is_ignored(EmploymentCheckType checkType)
        {
            // arrange
            var existingCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _sutModel.ApplicationApprenticeshipId,
                CorrelationId = Guid.NewGuid(),
                CheckType = checkType,
                MinimumDate = DateTime.Today.AddDays(-20),
                MaximumDate = DateTime.Today,
                CreatedDateTime = DateTime.Today.AddDays(-5)
            };

            _sutModel.EmploymentCheckModels.Add(existingCheck) ;

            _sut = Sut(_sutModel);

            // act
            _sut.UpdateEmploymentCheck(new EmploymentCheckResult(Guid.NewGuid(), EmploymentCheckResultType.Employed, DateTime.Today));

            // assert
            _sut.GetModel().EmploymentCheckModels.Single().Should().BeEquivalentTo(existingCheck);
        }

        [Test()]
        public void Then_a_new_result_is_stored()
        {
            // arrange
            var existingCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _sutModel.ApplicationApprenticeshipId,
                CorrelationId = Guid.NewGuid(),
                CheckType = EmploymentCheckType.EmployedAtStartOfApprenticeship,
                MinimumDate = DateTime.Today.AddDays(-20),
                MaximumDate = DateTime.Today,
                CreatedDateTime = DateTime.Today.AddDays(-5)
            };

            _sutModel.EmploymentCheckModels.Add(existingCheck);            

            _sut = Sut(_sutModel);
            _sut.GetModel().EmploymentCheckModels.Single().Result.HasValue.Should().BeFalse();
            _sut.GetModel().EmploymentCheckModels.Single().ResultDateTime.Should().BeNull();

            // act
            _sut.UpdateEmploymentCheck(new EmploymentCheckResult(existingCheck.CorrelationId, EmploymentCheckResultType.Employed, DateTime.Today));

            // assert
            _sut.GetModel().EmploymentCheckModels.Single().Result.Should().BeTrue();
            _sut.GetModel().EmploymentCheckModels.Single().ResultDateTime.Should().Be(DateTime.Today);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Then_a_previously_stored_result_is_updated(bool previousValue)
        {
            // arrange
            var existingCheck = new EmploymentCheckModel
            {
                Id = Guid.NewGuid(),
                ApprenticeshipIncentiveId = _sutModel.ApplicationApprenticeshipId,
                CorrelationId = Guid.NewGuid(),
                CheckType = EmploymentCheckType.EmployedAtStartOfApprenticeship,
                MinimumDate = DateTime.Today.AddDays(-20),
                MaximumDate = DateTime.Today,
                CreatedDateTime = DateTime.Today.AddDays(-5),
                Result = previousValue,
                ResultDateTime = DateTime.Today.AddDays(-5)
            };

            _sutModel.EmploymentCheckModels.Add(existingCheck);

            _sut = Sut(_sutModel);
            _sut.GetModel().EmploymentCheckModels.Single().Result.HasValue.Should().BeTrue();
            _sut.GetModel().EmploymentCheckModels.Single().Result.Value.Should().Be(previousValue);
            _sut.GetModel().EmploymentCheckModels.Single().ResultDateTime.Should().Be(DateTime.Today.AddDays(-5));

            // act
            _sut.UpdateEmploymentCheck(new EmploymentCheckResult(existingCheck.CorrelationId, previousValue ? EmploymentCheckResultType.NotEmployed : EmploymentCheckResultType.Employed, DateTime.Today));

            // assert
            _sut.GetModel().EmploymentCheckModels.Single().Result.Should().Be(!previousValue);
            _sut.GetModel().EmploymentCheckModels.Single().ResultDateTime.Should().Be(DateTime.Today);
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

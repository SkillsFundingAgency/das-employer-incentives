using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests.ValidationOverrideTests
{
    internal class WhenExpireValidationOverrides
    {
        private Fixture _fixture;
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;

        private ValidationOverrideModel _yesterdayOverride;
        private ValidationOverrideModel _todayOverride;
        private ValidationOverrideModel _tommorrowOverride;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture
                .Build<ApprenticeshipIncentiveModel>()
                .Without(i => i.ValidationOverrideModels)
                .Create();

            _yesterdayOverride = _fixture
                  .Build<ValidationOverrideModel>()
                  .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                  .With(p => p.ExpiryDate, DateTime.UtcNow.AddDays(-1))
                  .Create();

            _todayOverride = _fixture
                  .Build<ValidationOverrideModel>()
                  .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                  .With(p => p.ExpiryDate, DateTime.UtcNow)
                  .Create();

            _tommorrowOverride = _fixture
                  .Build<ValidationOverrideModel>()
                  .With(p => p.ApprenticeshipIncentiveId, _sutModel.Id)
                  .With(p => p.ExpiryDate, DateTime.UtcNow.AddDays(1))
                  .Create();

            _sutModel.ValidationOverrideModels.Add(_yesterdayOverride);
            _sutModel.ValidationOverrideModels.Add(_todayOverride);
            _sutModel.ValidationOverrideModels.Add(_tommorrowOverride);
            _sut = Sut(_sutModel);
        }        

        [Test]
        public void Then_existing_ValidationOverrides_are_removed_when_they_have_expired_today()
        {
            // Arrange
            var expireFrom = DateTime.Now;

            // Act
            _sut.ExpireValidationOverrides(expireFrom);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Count.Should().Be(1);
            _sut.GetModel().ValidationOverrideModels.Single().Id.Should().Be(_tommorrowOverride.Id);
        }

        [Test]
        public void Then_existing_ValidationOverrides_are_removed_when_they_have_expired()
        {
            // Arrange
            var expireFrom = DateTime.Now.AddDays(1);

            // Act
            _sut.ExpireValidationOverrides(expireFrom);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Count.Should().Be(0);
        }

        [Test]
        public void Then_existing_ValidationOverrides_are_not_removed_when_they_have_not_expired()
        {
            // Arrange
            var expireFrom = DateTime.Now.AddDays(-2);

            // Act
            _sut.ExpireValidationOverrides(expireFrom);

            // Assert
            _sut.GetModel().ValidationOverrideModels.Count.Should().Be(3);
        }

        [Test]
        public void Then_a_ValidationOverrideDeleted_event_is_raised_when_an_existing_ValidationOverrides_has_expired()
        {
            // Arrange
            var expireFrom = DateTime.Now.AddDays(-1);

            // Act
            _sut.ExpireValidationOverrides(expireFrom);

            // Assert
            var raisedEvent = _sut.FlushEvents().Single(e => e is ValidationOverrideDeleted) as ValidationOverrideDeleted;
            raisedEvent.Should().NotBeNull();
            raisedEvent.ValidationOverrideId.Should().Be(_yesterdayOverride.Id);
            raisedEvent.ApprenticeshipIncentiveId.Should().Be(_yesterdayOverride.ApprenticeshipIncentiveId);
            raisedEvent.ValidationOverrideStep.Should().Be(new ValidationOverrideStep(_yesterdayOverride.Step, _yesterdayOverride.ExpiryDate));
            raisedEvent.ServiceRequest.Should().BeNull();
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
 }


using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenSettingActualStartDate
    {
        private ApprenticeshipIncentive _sut;
        private ApprenticeshipIncentiveModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            _sutModel = _fixture.Create<ApprenticeshipIncentiveModel>();
            _sutModel.PendingPaymentModels = new List<PendingPaymentModel>(_fixture.CreateMany<PendingPaymentModel>(3));

            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_the_start_date_is_updated()
        {
            var newStartDate = _fixture.Create<DateTime>();
            
            _sut.SetActualStartDate(newStartDate);

            _sutModel.ActualStartDate.Should().Be(newStartDate);
        }

        [Test]
        public void Then_the_earnings_are_removed_when_the_start_date_has_changed()
        {
            var newStartDate = _fixture.Create<DateTime>();

            _sut.SetActualStartDate(newStartDate);

            _sutModel.PendingPaymentModels.Should().BeEmpty();
        }

        [Test]
        public void Then_earnings_are_not_removed_when_the_start_date_has_not_changed()
        {
            var newStartDate = _sutModel.ActualStartDate;

            _sut.SetActualStartDate(newStartDate);

            _sutModel.PendingPaymentModels.Count.Should().Be(3);
        }

        private ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    [TestFixture]
    public class WhenSettingActualStartDate
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentive _sut;
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
            
            _sut.SetStartDate(newStartDate);

            _sutModel.StartDate.Should().Be(newStartDate);
        }

        private ApprenticeshipIncentives.ApprenticeshipIncentive Sut(ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(model.Id, model);
        }
    }
}

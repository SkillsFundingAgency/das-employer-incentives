using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.Factories.ApprenticeshipIncentiveTests
{
    public class WhenExists
    {
        private ApprenticeshipIncentiveFactory _sut;
        private Fixture _fixture;
        private ApprenticeshipIncentiveModel _model;
        private Guid _id;

        [SetUp]
        public void Arrange()
        {
            _sut = new ApprenticeshipIncentiveFactory();
            _fixture = new Fixture();
            _id = _fixture.Create<Guid>();
            _model = _fixture.Create<ApprenticeshipIncentiveModel>();
        }

        [Test]
        public void Then_the_root_properties_are_mapped()
        {
            // Act
            var incentive = _sut.GetExisting(_id, _model);

            // Assert
            incentive.Should().BeEquivalentTo(_model, opt => 
            opt.Excluding(x => x.PendingPaymentModels)
            .Excluding(x => x.ApplicationApprenticeshipId)
            .Excluding(x => x.PaymentModels)
            .Excluding(x => x.ClawbackPaymentModels));
        }

        [Test]
        public void Then_the_pending_payments_are_mapped()
        {
            // Act
            var incentive = _sut.GetExisting(_id, _model);

            // Assert
            incentive.PendingPayments
                .Should()
                .BeEquivalentTo(
                    _model.PendingPaymentModels,
                    opt => opt.Excluding(x => x.ApprenticeshipIncentiveId)
                              .Excluding(x => x.CalculatedDate)
                              .Excluding(x => x.PendingPaymentValidationResultModels));
        }

        [Test]
        public void Then_the_pending_payment_validationResults_are_mapped()
        {
            // Act
            var pendingPayments = _sut.GetExisting(_id, _model).PendingPayments;

            // Assert
            pendingPayments.ToList().ForEach(p =>
                p.PendingPaymentValidationResults.Should()
                .BeEquivalentTo(
                    _model.PendingPaymentModels.Single(m => m.Id == p.Id).PendingPaymentValidationResultModels)
                );
        }

        [Test]
        public void Then_the_payments_are_mapped()
        {
            // Act
            var incentive = _sut.GetExisting(_id, _model);

            // Assert
            incentive.Payments
                .Should()
                .BeEquivalentTo(
                    _model.PaymentModels,
                    opt => opt.Excluding(x => x.ApprenticeshipIncentiveId)
                        .Excluding(x => x.CalculatedDate)
                        .Excluding(x => x.PendingPaymentId));
        }

        [Test]
        public void Then_the_clawbacks_are_mapped()
        {
            // Act
            var incentive = _sut.GetExisting(_id, _model);

            // Assert
            incentive.Clawbacks
                .Should()
                .BeEquivalentTo(
                    _model.ClawbackPaymentModels,
                    opt => opt.Excluding(x => x.ApprenticeshipIncentiveId)
                        .Excluding(x => x.PendingPaymentId)
                        .Excluding(x => x.Amount)
                        .Excluding(x => x.CreatedDate)
                        .Excluding(x => x.SubnominalCode)
                        .Excluding(x => x.PaymentId)
                        );
        }
    }
}

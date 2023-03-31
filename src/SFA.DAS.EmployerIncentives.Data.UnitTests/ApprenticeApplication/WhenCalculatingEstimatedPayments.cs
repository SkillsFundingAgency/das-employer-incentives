using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeApplication
{
    [TestFixture]
    public class WhenCalculatingEstimatedPayments
    {
        private Fixture _fixture;
        private Mock<IDateTimeService> _mockDateTimeService;
        private ApprenticeshipIncentives.Models.ApprenticeApplication _sut;
        private DateTime? _paymentDate;

        [SetUp]
        public void Arrange()
        {
            _mockDateTimeService = new Mock<IDateTimeService>();
            _fixture = new Fixture();
            _sut = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeApplication>()
                .With(x => x.FirstPaymentDate, new DateTime(DateTime.Now.Year, 1, 10))
                .With(x => x.SecondPaymentDate, new DateTime(DateTime.Now.Year, 1, 10))
                .Create();
        }

        [TestCase(EarningType.FirstPayment)]
        [TestCase(EarningType.SecondPayment)]
        public void Then_the_payment_is_estimated_when_the_payment_has_not_been_made(EarningType earningType)
        {
            _paymentDate = null;
            _sut = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeApplication>()
                .Without(x => x.FirstPaymentDate)
                .Without(x => x.SecondPaymentDate)
                .Create();

            for (int i = 1; i < 31; i++)
            {
                Then_the_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, earningType);
            }
            Assert.Pass();
        }

        [TestCase(EarningType.FirstPayment)]
        [TestCase(EarningType.SecondPayment)]
        public void Then_first_payment_is_not_estimated_when_the_payment_has_been_made_and_the_current_date_is_day_27_or_greater(EarningType earningType)
        {
            _paymentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10);
            for (int i = 27; i < 31; i++)
            {
                Then_the_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, false, earningType);
            }
            Assert.Pass();
        }

        [TestCase(EarningType.FirstPayment)]
        [TestCase(EarningType.SecondPayment)]
        public void Then_first_payment_is_estimated_when_the_payment_has_been_made_and_the_current_date_is_less_than_day_27(EarningType earningType)
        {
            _paymentDate = new DateTime(DateTime.Now.Year, 1, 10);
            for (int i = 1; i < 27; i++)
            {
                Then_the_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(i, true, earningType);
            }
            Assert.Pass();
        }

        private void Then_the_payment_is_as_expected_for_the_payment_and_the_calculated_date_is_set(
            int day,
            bool expected,
            EarningType earningType)
        {
            // Arrange            
            if (_paymentDate.HasValue)
            {
                _mockDateTimeService.Setup(m => m.UtcNow()).Returns(new DateTime(_paymentDate.Value.Year, 1, day));
            }
            else
            {
                _mockDateTimeService.Setup(m => m.UtcNow()).Returns(new DateTime(DateTime.Now.Year, 1, day));
            }

            // Act
            var isEstimated = _sut.IsPaymentEstimated(earningType, _mockDateTimeService.Object);

            // Assert
            isEstimated.Should().Be(expected);
        }
    }
}

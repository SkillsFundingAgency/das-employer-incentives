using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeApplication
{
    [TestFixture]
    public class WhenCalculatingIsIncentiveCompleted
    {
        private Fixture _fixture;
        private Mock<IDateTimeService> _mockDateTimeService;
        private ApprenticeshipIncentives.Models.ApprenticeApplication _sut;

        [SetUp]
        public void Arrange()
        {
            _mockDateTimeService = new Mock<IDateTimeService>();
            _mockDateTimeService.Setup(x => x.Now()).Returns(DateTime.Today);
            _fixture = new Fixture();
            _sut = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeApplication>()
                .With(x => x.FirstPaymentDate, DateTime.Today.AddMonths(-14).AddDays(90))
                .With(x => x.SecondPaymentDate, DateTime.Today.AddMonths(-14).AddDays(365))
                .Create();
        }

        [Test]
        public void Then_the_incentive_is_completed_if_the_stopped_date_is_in_the_past_and_after_the_second_payment_date()
        {
            // Arrange
            _sut.LearningStoppedDate = _sut.SecondPaymentDate.Value.AddDays(10);

            // Act
            var isCompleted = _sut.IsIncentiveCompleted(_mockDateTimeService.Object);

            // Assert
            isCompleted.Should().BeTrue();
        }

        [Test]
        public void Then_the_incentive_is_not_completed_if_the_stopped_date_is_in_the_past_and_before_the_second_payment_date()
        {
            // Arrange
            _sut.LearningStoppedDate = _sut.SecondPaymentDate.Value.AddDays(-1);

            // Act
            var isCompleted = _sut.IsIncentiveCompleted(_mockDateTimeService.Object);

            // Assert
            isCompleted.Should().BeFalse();
        }

        [Test]
        public void Then_the_incentive_is_not_completed_if_the_stopped_date_is_in_the_future()
        {
            // Arrange
            _sut.LearningStoppedDate = DateTime.Today.AddDays(1);

            // Act
            var isCompleted = _sut.IsIncentiveCompleted(_mockDateTimeService.Object);

            // Assert
            isCompleted.Should().BeFalse();
        }

        [Test]
        public void Then_the_incentive_is_not_completed_if_the_stopped_date_is_not_set()
        {
            // Arrange
            _sut.LearningStoppedDate = null;

            // Act
            var isCompleted = _sut.IsIncentiveCompleted(_mockDateTimeService.Object);

            // Assert
            isCompleted.Should().BeFalse();
        }

        [Test]
        public void Then_the_incentive_is_not_completed_if_the_second_payment_has_not_been_made()
        {
            // Arrange
            _sut.LearningStoppedDate = null;
            _sut.SecondPaymentDate = null;

            // Act
            var isCompleted = _sut.IsIncentiveCompleted(_mockDateTimeService.Object);

            // Assert
            isCompleted.Should().BeFalse();
        }
    }
}

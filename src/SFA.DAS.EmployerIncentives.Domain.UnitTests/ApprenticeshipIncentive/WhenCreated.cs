using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentiveTests
{
    public class WhenCreated
    {
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_the_id_is_set()
        {
            // Arrange
            var id = _fixture.Create<Guid>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(id, _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.Id.Should().Be(id);
            incentive.GetModel().Id.Should().Be(id);
        }

        [Test]
        public void Then_the_account_is_set()
        {
            // Arrange
            var account = _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), account, _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.Account.Should().Be(account);
            incentive.GetModel().Account.Id.Should().Be(account.Id);
        }

        [Test]
        public void Then_the_apprenticeshipId_is_set()
        {
            // Arrange
            var apprenticeshipId = _fixture.Create<Guid>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), apprenticeshipId, _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.GetModel().ApplicationApprenticeshipId.Should().Be(apprenticeshipId);
        }

        [Test]
        public void Then_the_startDate_is_set()
        {
            // Arrange
            var plannedStartDate = _fixture.Create<DateTime>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), plannedStartDate, _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.StartDate.Should().Be(plannedStartDate);
            incentive.GetModel().StartDate.Should().Be(plannedStartDate);
        }

        [Test]
        public void Then_the_submittedDate_is_set()
        {
            // Arrange
            var submittedDate = _fixture.Create<DateTime>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), submittedDate, _fixture.Create<string>());

            // Assert
            incentive.GetModel().SubmittedDate.Should().Be(submittedDate);
        }

        [Test]
        public void Then_the_submittedByEmail_is_set()
        {
            // Arrange
            var submittedByEmail = _fixture.Create<string>();

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), submittedByEmail);

            // Assert
            incentive.GetModel().SubmittedByEmail.Should().Be(submittedByEmail);
        }

        [Test]
        public void Then_there_are_no_pending_payments()
        {
            // Arrange            

            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.PendingPayments.Count.Should().Be(0);
            incentive.GetModel().PendingPaymentModels.Count.Should().Be(0);
        }

        [TestCase(2020, 8, 1, 4)]
        [TestCase(2021, 1, 31, 4)]
        [TestCase(2021, 2, 1, 5)]
        [TestCase(2021, 3, 31, 5)]
        public void Then_the_minimum_agreement_version_is_set(int year, int month, int day, int minimumAgreementVersion)
        {
            // Arrange
            var plannedStartDate = new DateTime(year, month, day);
            // Act
            var incentive = new ApprenticeshipIncentiveFactory().CreateNew(_fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Account>(), _fixture.Create<ApprenticeshipIncentives.ValueTypes.Apprenticeship>(), plannedStartDate, _fixture.Create<DateTime>(), _fixture.Create<string>());

            // Assert
            incentive.MinimumAgreementVersion.Should().Be(minimumAgreementVersion);
        }
    }
}

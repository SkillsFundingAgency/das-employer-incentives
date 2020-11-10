using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Learner = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.Learner;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.LearnerDataRepository
{
    public class WhenGetByApprenticeshipIncentiveIdCalled
    {
        private ApprenticeshipIncentives.LearnerDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.LearnerDataRepository(_dbContext);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_learner_is_retrieved_with_the_next_pending_payment()
        {
            // Arrange
            var testLearner = _fixture.Create<Learner>();
            await _dbContext.AddAsync(testLearner);

            var payments = _fixture.Build<PendingPayment>()
                .With(pp => pp.ApprenticeshipIncentiveId, testLearner.ApprenticeshipIncentiveId)
                .Without(pp => pp.PaymentMadeDate) // null for "not paid"
                .CreateMany(5).ToArray();

            payments[0].DueDate = DateTime.Parse("01/09/2020", new CultureInfo("en-GB"));
            payments[0].PaymentMadeDate = DateTime.Parse("30/09/2020", new CultureInfo("en-GB"));
            payments[1].DueDate = DateTime.Parse("01/10/2020", new CultureInfo("en-GB")); // next pending payment
            payments[2].DueDate = DateTime.Parse("01/11/2020", new CultureInfo("en-GB"));
            payments[3].DueDate = DateTime.Parse("01/12/2020", new CultureInfo("en-GB"));
            payments[4].DueDate = DateTime.Parse("01/01/2021", new CultureInfo("en-GB"));

            foreach (var payment in payments)
            {
                await _dbContext.AddAsync(payment);
            }

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _sut.GetByApprenticeshipIncentiveId(testLearner.ApprenticeshipIncentiveId);

            // Assert
            result.ApprenticeshipIncentiveId.Should().Be(testLearner.ApprenticeshipIncentiveId);
            result.ApprenticeshipId.Should().Be(testLearner.ApprenticeshipId);
            result.CreatedDate.Should().Be(testLearner.CreatedDate);
            result.Id.Should().Be(testLearner.Id);
            result.SubmissionFound.Should().Be(testLearner.SubmissionFound);
            result.Ukprn.Should().Be(testLearner.Ukprn);
            result.UniqueLearnerNumber.Should().Be(testLearner.ULN);

            result.SubmissionData.StartDate.Should().Be(testLearner.StartDate);
            Debug.Assert(testLearner.SubmissionDate != null, "testLearner.SubmissionDate != null");
            result.SubmissionData.SubmissionDate.Should().Be(testLearner.SubmissionDate.Value);
            result.SubmissionData.LearningFoundStatus.LearningFound.Should().Be(testLearner.LearningFound != null && testLearner.LearningFound.Value);

            result.NextPendingPayment.CollectionPeriod.Should().Be(payments[1].PeriodNumber);
            result.NextPendingPayment.CollectionYear.Should().Be(payments[1].PaymentYear);
            result.NextPendingPayment.DueDate.Should().Be(payments[1].DueDate);
        }
    }
}

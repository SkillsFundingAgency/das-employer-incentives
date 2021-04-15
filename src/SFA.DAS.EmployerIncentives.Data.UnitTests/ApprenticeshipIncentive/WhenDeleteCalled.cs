using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenDeleteCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_deleted_from_the_database()
        {
            // Arrange
            var incentive = _fixture.Create<ApprenticeshipIncentiveModel>();
            var cpData = _fixture.Build<ApprenticeshipIncentives.Models.CollectionPeriod>()
                .With(x => x.Active, true)
                .With(x => x.PeriodNumber, 2)
                .With(x => x.CalendarMonth, 9)
                .With(x => x.CalendarYear, 2020)
                .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                .With(x => x.AcademicYear, "2021")
                .Create();

            var cp = new CollectionPeriod(cpData.PeriodNumber, cpData.CalendarMonth, cpData.CalendarYear,
                cpData.EIScheduledOpenDateUTC, cpData.CensusDate, Convert.ToInt16(cpData.AcademicYear), true, false);

            var validationResults = _fixture.Build<PendingPaymentValidationResultModel>()
                .With(x => x.CollectionPeriod, cp)
                .CreateMany(4).ToList();

            var pendingPayments = _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, incentive.Id)
                .CreateMany(2).ToList();

            pendingPayments[0].PendingPaymentValidationResultModels = new[] { validationResults[0], validationResults[1] };
            pendingPayments[1].PendingPaymentValidationResultModels = new[] { validationResults[2], validationResults[3] };

            var payments = _fixture.Build<PaymentModel>()
                .With(x => x.ApprenticeshipIncentiveId, incentive.Id)
                .CreateMany(2).ToList();

            incentive.PendingPaymentModels = pendingPayments;
            incentive.PaymentModels = payments;

            await _dbContext.AddAsync(cpData);
            await _sut.Add(incentive);
            await _dbContext.SaveChangesAsync();

            // Act
            var storedIncentive = await _sut.Get(incentive.Id);
            await _sut.Delete(storedIncentive);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ApprenticeshipIncentives.Should().BeEmpty();
            _dbContext.PendingPayments.Should().BeEmpty();
            _dbContext.PendingPaymentValidationResults.Should().BeEmpty();
            _dbContext.Payments.Should().BeEmpty();
        }
    }
}

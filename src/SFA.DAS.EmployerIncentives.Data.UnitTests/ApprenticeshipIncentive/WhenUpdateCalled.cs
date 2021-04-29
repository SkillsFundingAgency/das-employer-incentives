using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using CollectionPeriod = SFA.DAS.EmployerIncentives.Domain.ValueObjects.CollectionPeriod;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenUpdateCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private ApprenticeshipIncentives.Models.CollectionPeriod _collectionPeriod;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
            await AddCollectionPeriod();
        }

        [TearDown]
        public void CleanUp() => _dbContext.Dispose();

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_updated_with_new_values()
        {
            // Arrange
            var expected = await SaveAndGetApprenticeshipIncentive();

            expected.Account = _fixture.Create<Domain.ApprenticeshipIncentives.ValueTypes.Account>();
            expected.Apprenticeship = _fixture.Create<Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship>();
            expected.Phase = new Domain.ValueObjects.IncentivePhase(Enums.Phase.Phase1_1);

            var pendingPayments = _fixture.Build<PendingPaymentModel>().With(
                x => x.ApprenticeshipIncentiveId, expected.Id).CreateMany().ToList();
            expected.PendingPaymentModels = pendingPayments;

            var validationResults = _fixture.CreateMany<PendingPaymentValidationResultModel>().ToList();
            expected.PendingPaymentModels.First().PendingPaymentValidationResultModels = validationResults;

            foreach (var pendingPaymentModel in expected.PendingPaymentModels)
            {
                var payment = _fixture.Build<PaymentModel>()
                    .With(x => x.ApprenticeshipIncentiveId, expected.Id)
                    .With(x => x.PendingPaymentId, pendingPaymentModel.Id).Create();
                expected.PaymentModels.Add(payment);
            }

            var clawbackPayments = _fixture.Build<ClawbackPaymentModel>().With(
                x => x.ApprenticeshipIncentiveId, expected.Id).CreateMany().ToList();
            expected.ClawbackPaymentModels = clawbackPayments;

            // Act
            await _sut.Update(expected);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(1);
            _dbContext.ApprenticeshipIncentives.Count(
                x => x.AccountId == expected.Account.Id &&
                     x.ApprenticeshipId == expected.Apprenticeship.Id &&
                     x.FirstName == expected.Apprenticeship.FirstName &&
                     x.LastName == expected.Apprenticeship.LastName &&
                     x.DateOfBirth == expected.Apprenticeship.DateOfBirth &&
                     x.ULN == expected.Apprenticeship.UniqueLearnerNumber &&
                     x.EmployerType == expected.Apprenticeship.EmployerType &&
                     x.SubmittedByEmail == expected.SubmittedByEmail &&
                     x.SubmittedDate == expected.SubmittedDate &&
                     x.CourseName == expected.Apprenticeship.CourseName &&
                     x.Phase == expected.Phase.Identifier
                )
                .Should().Be(1);

            var savedPendingPayments = _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedPendingPayments.Should().BeEquivalentTo(pendingPayments, opt => opt
                .Excluding(x => x.Account)
                .Excluding(x => x.PendingPaymentValidationResultModels));

            var savedValidationResults = _dbContext.PendingPaymentValidationResults.Where(x =>
                x.PendingPaymentId == expected.PendingPaymentModels.First().Id);
            savedValidationResults.Should().BeEquivalentTo(validationResults, opt => opt
                .Excluding(x => x.CollectionPeriod)
            );

            _dbContext.PendingPayments.Count().Should().Be(expected.PendingPaymentModels.Count);
            _dbContext.Payments.Count().Should().Be(expected.PaymentModels.Count);

            foreach (var result in savedValidationResults)
            {
                result.PeriodNumber.Should()
                    .Be(validationResults.Single(x => x.Id == result.Id).CollectionPeriod.PeriodNumber);
                result.PaymentYear.Should()
                    .Be(validationResults.Single(x => x.Id == result.Id).CollectionPeriod.AcademicYear);
                result.CreatedDateUtc.Should().BeCloseTo(validationResults.Single(x => x.Id == result.Id).CreatedDateUtc, TimeSpan.FromMinutes(1));
            }

            var savedClawbackPayments = _dbContext.ClawbackPayments.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedClawbackPayments.Should().BeEquivalentTo(clawbackPayments, opt => opt
                .Excluding(x => x.Account).Excluding(x => x.CreatedDate));
        }

        [Test]
        public async Task Then_existing_pending_payments_are_updated()
        {
            // Arrange
            var expected = await SaveAndGetApprenticeshipIncentive();
            AddUpdateAndRemovePendingPaymentsAndValidationResults(expected);

            // Act
            await _sut.Update(expected);
            await _dbContext.SaveChangesAsync();

            // Assert 
            var result = await _sut.Get(expected.Id);
            result.Should().BeEquivalentTo(expected);
        }

        private async Task<ApprenticeshipIncentiveModel> SaveAndGetApprenticeshipIncentive()
        {
            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>().Without(m => m.BreakInLearnings).With(p => p.Phase, Enums.Phase.NotSet).Create();
            foreach (var pendingPayment in incentive.PendingPayments)
            {
                pendingPayment.PaymentYear = Convert.ToInt16(_collectionPeriod.AcademicYear);
                pendingPayment.PeriodNumber = _collectionPeriod.PeriodNumber;
                foreach (var validationResult in pendingPayment.ValidationResults)
                {
                    validationResult.PaymentYear = Convert.ToInt16(_collectionPeriod.AcademicYear);
                    validationResult.PeriodNumber = _collectionPeriod.PeriodNumber;
                }
            }

            await _dbContext.AddAsync(incentive);
            await _dbContext.SaveChangesAsync();
            return await _sut.Get(incentive.Id);
        }

        private async Task AddCollectionPeriod()
        {
            _collectionPeriod = _fixture.Build<ApprenticeshipIncentives.Models.CollectionPeriod>()
                .With(x => x.Active, true)
                .With(x => x.PeriodNumber, 2)
                .With(x => x.CalendarMonth, 9)
                .With(x => x.CalendarYear, 2020)
                .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                .With(x => x.AcademicYear, "2021")
                .Create();
            await _dbContext.AddAsync(_collectionPeriod);
        }

        private void AddUpdateAndRemovePendingPaymentsAndValidationResults(ApprenticeshipIncentiveModel expected)
        {
            expected.PendingPaymentModels.First().PendingPaymentValidationResultModels.First().Result
                = !expected.PendingPaymentModels.First().PendingPaymentValidationResultModels.First().Result;
            expected.PendingPaymentModels.First().Amount -= 250;
            expected.PaymentModels.First().Amount -= 250;
            var newPendingPayment = _fixture.Build<PendingPaymentModel>()
                .With(x => x.ApprenticeshipIncentiveId, expected.Id)
                .With(x => x.PaymentYear, _collectionPeriod.PeriodNumber)
                .With(x => x.PeriodNumber, _collectionPeriod.PeriodNumber)
                .Without(x => x.PendingPaymentValidationResultModels)
                .Create();

            var cp = new CollectionPeriod(_collectionPeriod.PeriodNumber, _collectionPeriod.CalendarMonth,
                _collectionPeriod.CalendarYear,
                _collectionPeriod.EIScheduledOpenDateUTC, _collectionPeriod.CensusDate,
                Convert.ToInt16(_collectionPeriod.AcademicYear), true);

            expected.PendingPaymentModels.Last().PendingPaymentValidationResultModels
                .Remove(expected.PendingPaymentModels.Last().PendingPaymentValidationResultModels.First());
            expected.PendingPaymentModels.Last().PendingPaymentValidationResultModels.Add(
                _fixture.Build<PendingPaymentValidationResultModel>()
                    .With(x => x.CollectionPeriod, cp)
                    .Create()
            );

            newPendingPayment.PendingPaymentValidationResultModels.Add(
                _fixture.Build<PendingPaymentValidationResultModel>()
                    .With(x => x.CollectionPeriod, cp)
                    .Create()
            );

            expected.PendingPaymentModels.Add(newPendingPayment);
        }
    }
}

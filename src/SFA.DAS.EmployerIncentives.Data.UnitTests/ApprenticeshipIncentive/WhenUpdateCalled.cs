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
    public class WhenUpdateCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private readonly Fixture _fixture = new Fixture();
        private EmployerIncentivesDbContext _dbContext;
        private ApprenticeshipIncentives.Models.CollectionCalendarPeriod _collectionCalendarPeriod;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>().UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
            await AddCollectionCalendarPeriod();
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
            expected.Phase = new IncentivePhase(Enums.Phase.Phase1);

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

            var employmentChecks = _fixture.Build<EmploymentCheckModel>().With(
                x => x.ApprenticeshipIncentiveId, expected.Id).CreateMany().ToList();
            expected.EmploymentCheckModels = employmentChecks;

            var validationOverrides = _fixture.Build<ValidationOverrideModel>().With(
                x => x.ApprenticeshipIncentiveId, expected.Id).CreateMany().ToList();
            expected.ValidationOverrideModels = validationOverrides;

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
                     x.EmploymentStartDate == expected.Apprenticeship.EmploymentStartDate &&
                     x.Phase == expected.Phase.Identifier &&
                     x.WithdrawnBy == expected.WithdrawnBy
                )
                .Should().Be(1);

            var savedPendingPayments = _dbContext.PendingPayments.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedPendingPayments.Should().BeEquivalentTo(pendingPayments, opt => opt
                .Excluding(x => x.Account)
                .Excluding(x => x.PendingPaymentValidationResultModels)
                .Excluding(x => x.CollectionPeriod));

            var savedValidationResults = _dbContext.PendingPaymentValidationResults.Where(x =>
                x.PendingPaymentId == expected.PendingPaymentModels.First().Id);

            _dbContext.PendingPayments.Count().Should().Be(expected.PendingPaymentModels.Count);
            _dbContext.Payments.Count().Should().Be(expected.PaymentModels.Count);

            foreach (var result in savedValidationResults)
            {
                result.Step.Should().Be(validationResults.Single(x => x.Id == result.Id).Step);
                result.OverrideResult.Should().Be(validationResults.Single(x => x.Id == result.Id).OverrideResult);                
                result.Result.Should().Be(validationResults.Single(x => x.Id == result.Id).ValidationResult);
                result.PeriodNumber.Should().Be(validationResults.Single(x => x.Id == result.Id).CollectionPeriod.PeriodNumber);
                result.PaymentYear.Should().Be(validationResults.Single(x => x.Id == result.Id).CollectionPeriod.AcademicYear);
                result.CreatedDateUtc.Should().BeCloseTo(validationResults.Single(x => x.Id == result.Id).CreatedDateUtc, TimeSpan.FromMinutes(1));
            }

            var savedClawbackPayments = _dbContext.ClawbackPayments.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedClawbackPayments.Should().BeEquivalentTo(clawbackPayments, opt => opt
                .Excluding(x => x.Account).Excluding(x => x.CreatedDate).Excluding(x => x.CollectionPeriod));

            foreach (var savedClawbackPayment in savedClawbackPayments)
            {
                var expectedClawbackPayment = clawbackPayments.Single(c => c.Id == savedClawbackPayment.Id);
                expectedClawbackPayment.CollectionPeriod.PeriodNumber.Should().Be(savedClawbackPayment.CollectionPeriod);
                expectedClawbackPayment.CollectionPeriod.AcademicYear.Should().Be(savedClawbackPayment.CollectionPeriodYear);
            }

            var savedEmploymentChecks = _dbContext.EmploymentChecks.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedEmploymentChecks.Should().BeEquivalentTo(employmentChecks, opts => opts.Excluding(x => x.CreatedDateTime).Excluding(x => x.ResultDateTime).Excluding(x => x.UpdatedDateTime));

            var savedValidationOverrides = _dbContext.ValidationOverrides.Where(x => x.ApprenticeshipIncentiveId == expected.Id);
            savedValidationOverrides.Should().BeEquivalentTo(validationOverrides, opts => opts.Excluding(x => x.CreatedDate));
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

        [Test]
        public async Task Then_removed_break_in_learning_is_deleted()
        {
            // Arrange
            var expected = await SaveAndGetApprenticeshipIncentive();
            var lastBreakInLearning = expected.BreakInLearnings.Last();
            expected.BreakInLearnings.Remove(lastBreakInLearning);

            // Act
            await _sut.Update(expected);
            await _dbContext.SaveChangesAsync();

            // Assert 
            var result = await _sut.Get(expected.Id);
            result.BreakInLearnings.Count.Should().Be(2);
        }
        
        private async Task<ApprenticeshipIncentiveModel> SaveAndGetApprenticeshipIncentive()
        {
            var incentive = _fixture.Build<ApprenticeshipIncentives.Models.ApprenticeshipIncentive>()
                .With(p => p.Phase, Enums.Phase.NotSet)
                .Create();

            foreach (var breakInLearning in incentive.BreakInLearnings)
            {
                breakInLearning.EndDate = breakInLearning.StartDate.AddDays(_fixture.Create<int>());
            }

            foreach (var pendingPayment in incentive.PendingPayments)
            {
                pendingPayment.PaymentYear = Convert.ToInt16(_collectionCalendarPeriod.AcademicYear);
                pendingPayment.PeriodNumber = _collectionCalendarPeriod.PeriodNumber;
                foreach (var validationResult in pendingPayment.ValidationResults)
                {
                    validationResult.PaymentYear = Convert.ToInt16(_collectionCalendarPeriod.AcademicYear);
                    validationResult.PeriodNumber = _collectionCalendarPeriod.PeriodNumber;
                }
            }

            await _dbContext.AddAsync(incentive);
            await _dbContext.SaveChangesAsync();
            return await _sut.Get(incentive.Id);
        }

        private async Task AddCollectionCalendarPeriod()
        {
            _collectionCalendarPeriod = _fixture.Build<ApprenticeshipIncentives.Models.CollectionCalendarPeriod>()
                .With(x => x.Active, true)
                .With(x => x.PeriodNumber, 2)
                .With(x => x.CalendarMonth, 9)
                .With(x => x.CalendarYear, 2020)
                .With(x => x.EIScheduledOpenDateUTC, new DateTime(2020, 9, 6))
                .With(x => x.CensusDate, new DateTime(2020, 9, 30))
                .With(x => x.AcademicYear, "2021")
                .Without(x => x.MonthEndProcessingCompleteUTC)
                .Create();
            await _dbContext.AddAsync(_collectionCalendarPeriod);
        }

        private void AddUpdateAndRemovePendingPaymentsAndValidationResults(ApprenticeshipIncentiveModel expected)
        {
            expected.PendingPaymentModels.First().PendingPaymentValidationResultModels.First().ValidationResult
                = !expected.PendingPaymentModels.First().PendingPaymentValidationResultModels.First().ValidationResult;
            expected.PendingPaymentModels.First().Amount -= 250;
            expected.PaymentModels.First().Amount -= 250;
            var newPendingPayment = _fixture.Build<PendingPaymentModel>()
                .With(x => x.ApprenticeshipIncentiveId, expected.Id)
                .With(x => x.CollectionPeriod, new Domain.ValueObjects.CollectionPeriod(_collectionCalendarPeriod.PeriodNumber, Convert.ToInt16(_collectionCalendarPeriod.AcademicYear)))
                .Without(x => x.PendingPaymentValidationResultModels)
                .Create();

            var cp = new Domain.ValueObjects.CollectionPeriod(_collectionCalendarPeriod.PeriodNumber, Convert.ToInt16(_collectionCalendarPeriod.AcademicYear));

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

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenDeleteCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private ApprenticeshipIncentiveModel _testIncentive;

        [SetUp]
        public async Task Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _testIncentive = _fixture.Create<ApprenticeshipIncentiveModel>();
            _testIncentive.PendingPaymentModels = new List<PendingPaymentModel>()
            {
                _fixture
                .Build<PendingPaymentModel>()
                .With(p => p.ApprenticeshipIncentiveId, _testIncentive.Id)
                .With(p => p.PendingPaymentValidationResultModels, new List<PendingPaymentValidationResultModel>())
                .Create()
            };

            _testIncentive.PaymentModels = new List<PaymentModel>();
           
            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
            await _sut.Add(_testIncentive);
            await _dbContext.SaveChangesAsync();
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_apprenticeship_incentive_is_deleted_from_the_database()
        {
            // Arrange
            var storedIncentive = await _sut.Get(_testIncentive.Id);

            // Act
            await _sut.Delete(storedIncentive);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ApprenticeshipIncentives.Count().Should().Be(0);
            _dbContext.PendingPayments.Count().Should().Be(0);            
        }
    }
}

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentiveArchive
{
    public class WhenArchivePaymentModelCalled
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveArchiveRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_payment_model_is_added_to_the_data_store()
        {
            // Arrange
            var testPayment = _fixture
                .Build<PaymentModel>()
                .Create();

            // Act
            await _sut.Archive(testPayment);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ArchivedPayments.Count().Should().Be(1);

            var storedPayment = _dbContext.ArchivedPayments.Single();
            storedPayment.PaymentId.Should().Be(testPayment.Id);
            storedPayment.ApprenticeshipIncentiveId.Should().Be(testPayment.ApprenticeshipIncentiveId);
            storedPayment.PendingPaymentId.Should().Be(testPayment.PendingPaymentId);
            storedPayment.AccountId.Should().Be(testPayment.Account.Id);
            storedPayment.AccountLegalEntityId.Should().Be(testPayment.Account.AccountLegalEntityId);
            storedPayment.Amount.Should().Be(testPayment.Amount);
            storedPayment.CalculatedDate.Should().Be(testPayment.CalculatedDate);
            storedPayment.PaidDate.Should().Be(testPayment.PaidDate);
            storedPayment.SubnominalCode.Should().Be(testPayment.SubnominalCode);
            storedPayment.PaymentPeriod.Should().Be(testPayment.PaymentPeriod);
            storedPayment.PaymentYear.Should().Be(testPayment.PaymentYear);
            storedPayment.ArchiveDateUTC.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        }
    }
}

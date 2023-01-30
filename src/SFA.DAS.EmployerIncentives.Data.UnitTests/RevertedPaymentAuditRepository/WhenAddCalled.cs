using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.RevertedPaymentAuditRepository
{
    [TestFixture]
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.RevertedPaymentAuditRepository _sut;
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

            _sut = new ApprenticeshipIncentives.RevertedPaymentAuditRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_reverted_payment_audit_is_added_to_the_data_store()
        {
            // Arrange
            var testAudit = _fixture.Create<RevertedPaymentAudit>();

            // Act
            await _sut.Add(testAudit);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.RevertedPaymentAudits.Count().Should().Be(1);

            var storedAudit = _dbContext.RevertedPaymentAudits.Single();
            storedAudit.Id.Should().Be(testAudit.Id);
            storedAudit.ApprenticeshipIncentiveId.Should().Be(testAudit.ApprenticeshipIncentiveId);
            storedAudit.PaymentId.Should().Be(testAudit.PaymentId);
            storedAudit.Amount.Should().Be(testAudit.Amount);
            storedAudit.CalculatedDate.Should().Be(testAudit.CalculatedDate);
            storedAudit.PaidDate.Should().Be(testAudit.PaidDate);
            storedAudit.PaymentPeriod.Should().Be(testAudit.PaymentPeriod);
            storedAudit.PaymentYear.Should().Be(testAudit.PaymentYear);
            storedAudit.PendingPaymentId.Should().Be(testAudit.PendingPaymentId);
            storedAudit.VrfVendorId.Should().Be(testAudit.VrfVendorId);
            storedAudit.ServiceRequestTaskId.Should().Be(testAudit.ServiceRequest.TaskId);
            storedAudit.ServiceRequestDecisionReference.Should().Be(testAudit.ServiceRequest.DecisionReference);
            storedAudit.ServiceRequestCreatedDate.Should().Be(testAudit.ServiceRequest.Created);
        }
    }
}

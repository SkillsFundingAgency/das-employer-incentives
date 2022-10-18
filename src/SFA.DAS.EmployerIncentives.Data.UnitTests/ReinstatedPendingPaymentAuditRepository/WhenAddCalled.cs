using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ReinstatedPendingPaymentAuditRepository
{
    [TestFixture]
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.ReinstatedPendingPaymentAuditRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ReinstatedPendingPaymentAuditRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_reinstated_pending_payment_audit_is_added_to_the_data_store()
        {
            // Arrange
            var testAudit = _fixture.Create<ReinstatedPendingPaymentAudit>();

            // Act
            await _sut.Add(testAudit);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.ReinstatedPendingPaymentAudits.Count().Should().Be(1);

            var storedAudit = _dbContext.ReinstatedPendingPaymentAudits.Single();
            storedAudit.Id.Should().Be(testAudit.Id);
            storedAudit.ApprenticeshipIncentiveId.Should().Be(testAudit.ApprenticeshipIncentiveId);
            storedAudit.PendingPaymentId.Should().Be(testAudit.PendingPaymentId);
            storedAudit.ServiceRequestTaskId.Should().Be(testAudit.ReinstatePaymentRequest.TaskId);
            storedAudit.ServiceRequestDecisionReference.Should().Be(testAudit.ReinstatePaymentRequest.DecisionReference);
            storedAudit.ServiceRequestCreatedDate.Should().Be(testAudit.ReinstatePaymentRequest.Created);
            storedAudit.Process.Should().Be(testAudit.ReinstatePaymentRequest.Process);
        }
    }
}

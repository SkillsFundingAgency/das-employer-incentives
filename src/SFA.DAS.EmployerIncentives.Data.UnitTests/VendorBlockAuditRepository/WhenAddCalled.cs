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

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.VendorBlockAuditRepository
{
    [TestFixture]
    public class WhenAddCalled
    {
        private Account.VendorBlockAuditRepository _sut;
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

            _sut = new Account.VendorBlockAuditRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_the_vendor_block_audit_is_added_to_the_data_store()
        {
            // Arrange
            var testAudit = _fixture
                .Build<VendorBlockRequestAudit>()
                .Create();

            // Act
            await _sut.Add(testAudit);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.VendorBlockAudits.Count().Should().Be(1);

            var storedAudit = _dbContext.VendorBlockAudits.Single();
            storedAudit.Id.Should().Be(testAudit.Id);
            storedAudit.VrfVendorId.Should().Be(testAudit.VrfVendorId);
            storedAudit.VendorBlockEndDate.Should().Be(testAudit.VendorBlockEndDate);
            storedAudit.CreatedDateTime.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 1, 0));
            storedAudit.ServiceRequestCreatedDate.Should().Be(testAudit.ServiceRequest.Created);
            storedAudit.ServiceRequestDecisionReference.Should().Be(testAudit.ServiceRequest.DecisionReference);
            storedAudit.ServiceRequestTaskId.Should().Be(testAudit.ServiceRequest.TaskId);
        }
    }
}

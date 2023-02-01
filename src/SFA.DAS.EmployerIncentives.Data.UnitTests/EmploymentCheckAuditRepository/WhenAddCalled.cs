using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.EmploymentCheckAuditRepository
{
    public class WhenAddCalled
    {
        private ApprenticeshipIncentives.EmploymentCheckAuditRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.EmploymentCheckAuditRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_employment_check_audit_is_added_to_the_data_store()
        {
            // Arrange
            var testAudit = _fixture
                .Build<EmploymentCheckRequestAudit>()
                .Create();
            
            // Act
            await _sut.Add(testAudit);

            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.EmploymentCheckAudits.Count().Should().Be(1);

            var storedAudit = _dbContext.EmploymentCheckAudits.Single();
            storedAudit.Id.Should().Be(testAudit.Id);
            storedAudit.ApprenticeshipIncentiveId.Should().Be(testAudit.ApprenticeshipIncentiveId);
            storedAudit.CheckType.Should().Be(testAudit.CheckType.ToString());
            storedAudit.CreatedDateTime.Should().BeCloseTo(DateTime.UtcNow, new TimeSpan(0, 1, 0));
            storedAudit.ServiceRequestCreatedDate.Should().Be(testAudit.ServiceRequest.Created);
            storedAudit.ServiceRequestDecisionReference.Should().Be(testAudit.ServiceRequest.DecisionReference);
            storedAudit.ServiceRequestTaskId.Should().Be(testAudit.ServiceRequest.TaskId);
        }
    }
}

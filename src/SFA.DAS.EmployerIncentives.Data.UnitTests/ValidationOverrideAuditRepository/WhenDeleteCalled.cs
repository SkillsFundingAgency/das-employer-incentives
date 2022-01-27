﻿using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ValidationOverrideAuditRepository
{
    public class WhenDeleteCalled
    {
        private ApprenticeshipIncentives.ValidationOverrideAuditRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ValidationOverrideAuditRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
        }

        [TearDown]
        public void CleanUp()
        {            
            _dbContext.Dispose();
         }

        [Test]
        public async Task Then_the_existing_validation_override_audit_is_marked_as_deleted()
        {
            // Arrange
            var testAudit = _fixture
                .Build<ValidationOverrideStepAudit>()
                .Create();
            
            await _sut.Add(testAudit);

            await _dbContext.SaveChangesAsync();
            _dbContext.ValidationOverrideAudits.Count().Should().Be(1);
            var createdDate = _dbContext.ValidationOverrideAudits.Single().CreatedDateTime;

            // Act
            await _sut.Delete(testAudit.Id);

            // Assert
            _dbContext.ValidationOverrideAudits.Count().Should().Be(1);

            var storedAudit = _dbContext.ValidationOverrideAudits.Single();
            storedAudit.Id.Should().Be(testAudit.Id);
            storedAudit.ApprenticeshipIncentiveId.Should().Be(testAudit.ApprenticeshipIncentiveId);
            storedAudit.Step.Should().Be(testAudit.ValidationOverrideStep.ValidationType);
            storedAudit.ExpiryDate.Should().Be(testAudit.ValidationOverrideStep.ExpiryDate);
            storedAudit.CreatedDateTime.Should().Be(createdDate);
            storedAudit.DeletedDateTime.Should().BeCloseTo(DateTime.Now, new TimeSpan(0, 1, 0));
            storedAudit.ServiceRequestTaskId.Should().Be(testAudit.ServiceRequest.TaskId);
            storedAudit.ServiceRequestDecisionReference.Should().Be(testAudit.ServiceRequest.DecisionReference);
            storedAudit.ServiceRequestCreatedDate.Should().Be(testAudit.ServiceRequest.Created);
        }
    }
}

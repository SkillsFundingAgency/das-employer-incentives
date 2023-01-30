using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.IncentiveApplicationStatusAuditDataRepository
{
    public class WhenGetIncentiveApplicationStatusAuditData
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IIncentiveApplicationStatusAuditDataRepository _sut;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new IncentiveApplication.IncentiveApplicationStatusAuditDataRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public void Then_data_is_fetched_from_database()
        {
            // Arrange
            var applicationId = Guid.NewGuid();

            var audits = _fixture.Build<Models.IncentiveApplicationStatusAudit>()
                .With(x => x.IncentiveApplicationApprenticeshipId, applicationId)
                .CreateMany<Models.IncentiveApplicationStatusAudit>(2).ToArray();

            _context.IncentiveApplicationStatusAudits.AddRange(audits);
           
            _context.SaveChanges();

            // Act
            var actual = _sut.GetByApplicationApprenticeshipId(applicationId);

            //Assert
            actual.Should().BeEquivalentTo(audits.ToList());
            
        }

        [Test]
        public async Task Then_the_IncentiveApplicationAudit_data_is_added_to_the_database()
        {
            // Arrange
            var expected = _fixture.Create<IncentiveApplicationAudit>();

            // Act 
            await _sut.Add(expected);
            _context.SaveChanges();
            var actual = _context.IncentiveApplicationStatusAudits.FirstOrDefault(x => x.Id == expected.Id);
           

            // Assert
            actual.Should().NotBeNull();
            actual.Id.Should().Be(expected.Id);
            actual.IncentiveApplicationApprenticeshipId.Should().Be(actual.IncentiveApplicationApprenticeshipId);
            actual.Process.Should().Be(expected.Process);

        }
    }
}

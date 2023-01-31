using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using PendingPayment = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives.PendingPayment;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.PendingPaymentQueryRepository
{
    public class WhenGetPendingPaymentCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private IQueryRepository<PendingPayment> _sut;
        private Mock<IServiceProvider> _mockServiceProvider;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();
            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options, _mockServiceProvider.Object);

            _sut = new ApprenticeshipIncentives.PendingPaymentQueryRepository(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [Test]
        public async Task Then_data_is_fetched_from_database()
        {
            // Arrange
            var pendingPayment = _fixture.Create<ApprenticeshipIncentives.Models.PendingPayment>();
            
            _context.PendingPayments.Add(pendingPayment);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == pendingPayment.Id);

            //Assert
            actual.Should().BeEquivalentTo(pendingPayment, opts => opts.ExcludingMissingMembers());
        }

        [Test]
        public async Task Then_null_is_returned_when_the_application_is_not_found()
        {
            // Arrange
            var allPendingPayments = _fixture.CreateMany<ApprenticeshipIncentives.Models.PendingPayment>(10).ToArray();
            
            _context.PendingPayments.AddRange(allPendingPayments);
            _context.SaveChanges();

            // Act
            var actual = await _sut.Get(x => x.Id == Guid.NewGuid());

            //Assert
            actual.Should().BeNull();
        }
    }
}
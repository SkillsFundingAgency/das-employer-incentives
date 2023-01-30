using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentiveArchive
{
    [TestFixture]
    public class WhenGetArchivePendingPaymentModelCalled
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
        public async Task Then_the_archived_pending_payment_is_retrieved_from_the_data_store()
        {
            // Arrange
            var pendingPaymentId = Guid.NewGuid();

            var archivedPendingPayment = _fixture.Build<ApprenticeshipIncentives.Models.Archive.PendingPayment>()
                .With(x => x.PendingPaymentId, pendingPaymentId)
                .Create();

            await _dbContext.AddAsync(archivedPendingPayment);
            await _dbContext.SaveChangesAsync();

            // Act
            var pendingPayment = await _sut.GetArchivedPendingPayment(pendingPaymentId);

            // Assert
            pendingPayment.Should().NotBeNull();
            pendingPayment.Id.Should().Be(pendingPaymentId);
            pendingPayment.ApprenticeshipIncentiveId.Should().Be(archivedPendingPayment.ApprenticeshipIncentiveId);
            pendingPayment.Amount.Should().Be(archivedPendingPayment.Amount);
            pendingPayment.EarningType.Should().Be(archivedPendingPayment.EarningType);
        }
    }
}

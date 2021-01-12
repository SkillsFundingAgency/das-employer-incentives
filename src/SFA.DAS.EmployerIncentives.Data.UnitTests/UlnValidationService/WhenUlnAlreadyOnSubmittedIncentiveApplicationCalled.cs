using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.UlnValidationService
{
    public class WhenUlnAlreadyOnSubmittedIncentiveApplicationCalled
    {
        private EmployerIncentivesDbContext _context;
        private Fixture _fixture;
        private Data.UlnValidationService _sut;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _context = new EmployerIncentivesDbContext(options);

            _sut = new Data.UlnValidationService(new Lazy<EmployerIncentivesDbContext>(_context));
        }

        [TearDown]
        public void CleanUp()
        {
            _context.Dispose();
        }

        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, true)]
        public async Task Then_an_eligible_apprenticeship_returns_expected_result_from_the_query(bool withdrawnByCompliance, bool withdrawnByEmployer, bool expected)
        {
            // Arrange
            var uln = _fixture.Create<long>();
            var application = _fixture.Build<Models.IncentiveApplication>()
                .With(a => a.Status, IncentiveApplicationStatus.Submitted).Create();
            var apprenticeship = _fixture.Build<Models.IncentiveApplicationApprenticeship>()
                .With(a => a.IncentiveApplicationId, application.Id)
                .With(a => a.ULN, uln)
                .With(a => a.WithdrawnByCompliance, withdrawnByCompliance)
                .With(a => a.WithdrawnByEmployer, withdrawnByEmployer)
                .Create();

            await _context.Applications.AddAsync(application);
            await _context.ApplicationApprenticeships.AddAsync(apprenticeship);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.UlnAlreadyOnSubmittedIncentiveApplication(uln);

            //Assert
            result.Should().Be(expected);
        }
    }
}
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.ApprenticeshipIncentive
{
    public class WhenFindingApprenticeshipByUlnWithinAccountLegalEntity
    {
        private ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository _sut;
        private Fixture _fixture;
        private EmployerIncentivesDbContext _dbContext;
        private ApprenticeshipIncentiveModel _testIncentive;
        private long _uln;
        private long _accountLegalEntityId;

        [SetUp]
        public async Task Arrange()
        {
            _fixture = new Fixture();
            _uln = _fixture.Create<long>();
            _accountLegalEntityId = _fixture.Create<long>();

            var options = new DbContextOptionsBuilder<EmployerIncentivesDbContext>()
                .UseInMemoryDatabase("EmployerIncentivesDbContext" + Guid.NewGuid()).Options;
            _dbContext = new EmployerIncentivesDbContext(options);

            _sut = new ApprenticeshipIncentives.ApprenticeshipIncentiveDataRepository(new Lazy<EmployerIncentivesDbContext>(_dbContext));
            await AddApprenticeshipIncentiveModel(_accountLegalEntityId, _uln);
            await AddApprenticeshipIncentiveModel(_accountLegalEntityId, _uln + 1);
            await AddApprenticeshipIncentiveModel(_accountLegalEntityId+1, _uln);
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Then_when_the_apprenticeship_incentive_exists_it_is_returned_from_the_database()
        {
            // Act
            var matchingIncentives = await _sut.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(_uln, _accountLegalEntityId);

            // Assert
            matchingIncentives.Should().NotBeNull();
            matchingIncentives.Count.Should().Be(1);
            matchingIncentives.First().Apprenticeship.UniqueLearnerNumber.Should().Be(_uln);
            matchingIncentives.First().Account.AccountLegalEntityId.Should().Be(_accountLegalEntityId);
        }

        [Test]
        public async Task Then_when_the_apprenticeship_incentive_does_not_exist_it_returns_an_empty_list()
        {
            // Act
            var matchingIncentives = await _sut.FindApprenticeshipIncentiveByUlnWithinAccountLegalEntity(_uln-1, _accountLegalEntityId-1);

            // Assert
            matchingIncentives.Count.Should().Be(0);
        }

        private async Task AddApprenticeshipIncentiveModel(long accountLegalEntityId, long uln)
        {
            var account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(_fixture.Create<long>(), accountLegalEntityId);
            var apprenticeship = new Apprenticeship(_fixture.Create<long>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<DateTime>(), uln,
                _fixture.Create<ApprenticeshipEmployerType>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

            var incentive = _fixture.Build<ApprenticeshipIncentiveModel>().With(x => x.Apprenticeship, apprenticeship)
                .With(x => x.Account, account).Create();

            await _sut.Add(incentive);
            await _dbContext.SaveChangesAsync();
        }
    }
}

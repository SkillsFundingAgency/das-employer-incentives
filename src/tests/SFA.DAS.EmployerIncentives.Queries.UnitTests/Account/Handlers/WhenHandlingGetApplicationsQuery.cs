using System;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Domain.Accounts.Models;
using SFA.DAS.EmployerIncentives.Queries.Account.GetApplications;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.Account.Handlers
{
    [TestFixture]
    public class WhenHandlingGetApplicationsQuery
    {
        private GetApplicationsQueryHandler _sut;
        private Mock<IApprenticeApplicationDataRepository> _applicationRepository;
        private Mock<IAccountDataRepository> _accountRepository;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _applicationRepository = new Mock<IApprenticeApplicationDataRepository>();
            _accountRepository = new Mock<IAccountDataRepository>();
            _sut = new GetApplicationsQueryHandler(_applicationRepository.Object, _accountRepository.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            // Arrange
            var query = _fixture.Create<GetApplicationsRequest>();
            var applicationsList = _fixture.CreateMany<ApprenticeApplicationDto>().ToList();
            var expectedResponse = new GetApplicationsResponse
            {
                ApprenticeApplications = applicationsList,
                BankDetailsStatus = Enums.BankDetailsStatus.NotSupplied,
                FirstSubmittedApplicationId = Guid.NewGuid()
            };

            _applicationRepository.Setup(x => x.GetList(query.AccountId, query.AccountLegalEntityId)).ReturnsAsync(applicationsList);

            var account = _fixture.Create<AccountModel>();
            var legalEntities = _fixture.CreateMany<LegalEntityModel>(1).ToList();
            legalEntities[0].AccountLegalEntityId = query.AccountLegalEntityId;
            legalEntities[0].BankDetailsStatus = Enums.BankDetailsStatus.NotSupplied;
            account.LegalEntityModels = new Collection<LegalEntityModel>(legalEntities);

            _accountRepository.Setup(x => x.Find(query.AccountId)).ReturnsAsync(account);

            _applicationRepository.Setup(x => x.GetFirstSubmittedApplicationId(query.AccountLegalEntityId)).ReturnsAsync(expectedResponse.FirstSubmittedApplicationId);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}

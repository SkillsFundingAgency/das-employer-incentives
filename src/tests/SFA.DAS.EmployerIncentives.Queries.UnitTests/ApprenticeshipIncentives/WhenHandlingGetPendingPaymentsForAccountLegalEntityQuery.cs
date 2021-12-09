using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.ApprenticeshipIncentives
{
    public class WhenHandlingGetPendingPaymentsForAccountLegalEntityQuery
    {
        private GetPendingPaymentsForAccountLegalEntityQueryHandler _sut;
        private Mock<IQueryRepository<PendingPaymentDto>> _repositoryMock;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _repositoryMock = new Mock<IQueryRepository<PendingPaymentDto>>();
            _sut = new GetPendingPaymentsForAccountLegalEntityQueryHandler(_repositoryMock.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_query_repository()
        {
            //Arrange
            var query = _fixture.Create<GetPendingPaymentsForAccountLegalEntityRequest>();
            var data = _fixture.CreateMany<PendingPaymentDto>().ToList();
            var expected = new GetPendingPaymentsForAccountLegalEntityResponse(data);

            _repositoryMock.Setup(x => x.GetList(dto => dto.AccountLegalEntityId == query.AccountLegalEntityId && !dto.PaymentMadeDate.HasValue && (dto.PaymentYear < query.CollectionPeriod.AcademicYear || (dto.PaymentYear == query.CollectionPeriod.AcademicYear && dto.PeriodNumber <= query.CollectionPeriod.PeriodNumber)))).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected, options => options.Excluding(o => o.Log));
        }

    }
}

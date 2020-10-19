using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPayableLegalEntities;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenGettingPayableLegalEntities
    {
        private Fixture _fixture;
        private CollectionPeriod _collectionPeriod;
        private GetPayableLegalEntities _sut;
        private List<PayableLegalEntityDto> _legalEntities;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _collectionPeriod = _fixture.Create<CollectionPeriod>();
            _legalEntities = _fixture.CreateMany<PayableLegalEntityDto>(3).ToList();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _mockQueryDispatcher
                .Setup(x =>
                    x.Send<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>(
                        It.IsAny<GetPayableLegalEntitiesRequest>()))
                .ReturnsAsync(new GetPayableLegalEntitiesResponse(_legalEntities));

            _sut = new GetPayableLegalEntities(_mockQueryDispatcher.Object, Mock.Of<ILogger<GetPayableLegalEntities>>());
        }

        [Test]
        public async Task Then_query_is_called_to_get_payable_legal_entities()
        {
            await _sut.Get(_collectionPeriod);

            _mockQueryDispatcher.Verify(
                x => x.Send<GetPayableLegalEntitiesRequest, GetPayableLegalEntitiesResponse>(
                    It.Is<GetPayableLegalEntitiesRequest>(p =>
                        p.CollectionPeriodMonth == _collectionPeriod.Month &&
                        p.CollectionPeriodYear == _collectionPeriod.Year)), Times.Once);
        }

        [Test]
        public async Task Then_query_returns_list_of_payable_legal_entities()
        {
            var list = await _sut.Get(_collectionPeriod);

            list.Should().BeEquivalentTo(_legalEntities);
        }
    }
}
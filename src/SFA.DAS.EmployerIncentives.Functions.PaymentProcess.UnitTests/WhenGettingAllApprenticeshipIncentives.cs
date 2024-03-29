using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Queries;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetApprenticeshipIncentives;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentProcess.UnitTests
{
    public class WhenGettingAllApprenticeshipIncentives
    {
        private Fixture _fixture;
        private GetAllApprenticeshipIncentives _sut;
        private List<ApprenticeshipIncentive> _apprenticeshipIncentives;
        private Mock<IQueryDispatcher> _mockQueryDispatcher;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _apprenticeshipIncentives = _fixture.CreateMany<ApprenticeshipIncentive>(3).ToList();
            _mockQueryDispatcher = new Mock<IQueryDispatcher>();
            _mockQueryDispatcher
                .Setup(x =>
                    x.Send<GetApprenticeshipIncentivesRequest, GetApprenticeshipIncentivesResponse>(
                        It.IsAny<GetApprenticeshipIncentivesRequest>()))
                .ReturnsAsync(new GetApprenticeshipIncentivesResponse(_apprenticeshipIncentives));

            _sut = new GetAllApprenticeshipIncentives(_mockQueryDispatcher.Object);
        }

        [Test]
        public async Task Then_query_is_called_to_get_apprenticeship_incentives()
        {
            await _sut.Get(null);

            _mockQueryDispatcher.Verify(x => x.Send<GetApprenticeshipIncentivesRequest, GetApprenticeshipIncentivesResponse>(
                    It.IsAny<GetApprenticeshipIncentivesRequest>()), Times.Once);
        }

        [Test]
        public async Task Then_query_returns_list_of_apprenticeship_incentives_ids()
        {
            var list = await _sut.Get(null);

            list.Should().BeEquivalentTo(_apprenticeshipIncentives.Select(x=> new ApprenticeshipIncentiveOutput { Id = x.Id, ULN = x.ULN }));
        }
    }
}
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Queries.NewApprenticeIncentive.GetApplication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Queries.UnitTests.NewApprenticeIncentive.Handlers
{
    public class WhenHandlingGetApplicationQuery
    {
        private GetApplicationQueryHandler _sut;
        private Mock<IIncentiveApplicationQueryRepository> _applicationRepository;
        private Mock<IIncentivePaymentProfilesService> _incentivePaymentProfilesService;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _applicationRepository = new Mock<IIncentiveApplicationQueryRepository>();

            _incentivePaymentProfilesService = new Mock<IIncentivePaymentProfilesService>();
            
            _sut = new GetApplicationQueryHandler(_applicationRepository.Object, _incentivePaymentProfilesService.Object);
        }

        [Test]
        public async Task Then_data_is_fetched_via_data_repository()
        {
            //Arrange
            var query = _fixture.Create<GetApplicationRequest>();
            var apprenticeship = _fixture.Build<IncentiveApplicationApprenticeshipDto>().With(x => x.Phase, Enums.Phase.Phase1).With(x => x.PlannedStartDate, new DateTime(2020, 9,1)).Create();
            var data = _fixture.Build<IncentiveApplicationDto>().With(x => x.Apprenticeships, new List<IncentiveApplicationApprenticeshipDto> { apprenticeship }).Create();
            var expected = new GetApplicationResponse(data);

            _applicationRepository.Setup(x => x.Get(It.IsAny<List<IncentivePaymentProfile>>(), dto => dto.Id == query.ApplicationId && dto.AccountId == query.AccountId)).ReturnsAsync(data);

            //Act
            var result = await _sut.Handle(query, CancellationToken.None);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}

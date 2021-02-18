using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Abstractions.Commands;
using SFA.DAS.EmployerIncentives.Api.Controllers;
using SFA.DAS.EmployerIncentives.Api.Types;
using SFA.DAS.EmployerIncentives.Commands.RefreshLegalEntities;
using SFA.DAS.EmployerIncentives.Commands.Services.AccountApi;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Api.UnitTests.Job
{
    [TestFixture]
    public class WhenRefreshLegalEntities
    {
        private JobCommandController _sut;
        private Mock<ICommandDispatcher> _mockCommandDispatcher;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockCommandDispatcher = new Mock<ICommandDispatcher>();
            _fixture = new Fixture();
            _sut = new JobCommandController(_mockCommandDispatcher.Object);

            _mockCommandDispatcher
                .Setup(m => m.Send(It.IsAny<RefreshLegalEntitiesCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task Then_a_AddLegalEntityCommand_command_is_dispatched()
        {
            // Arrange
            var request = _fixture.Build<JobRequest>().With(r => r.Type, JobType.RefreshLegalEntities).Create();
            var pageNumber = _fixture.Create<long>();
            var pageSize = _fixture.Create<long>();
            var accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10);
            var totalPages = _fixture.Create<long>();
            request.Data.Add("PageNumber", pageNumber);
            request.Data.Add("PageSize", pageSize);
            request.Data.Add("TotalPages", totalPages);
            request.Data.Add("AccountLegalEntities", JsonConvert.SerializeObject(accountLegalEntities));

            // Act
            await _sut.AddJob(request);

            // Assert
            _mockCommandDispatcher
                .Verify(m => m.Send(It.Is<RefreshLegalEntitiesCommand>(c =>
                    c.PageNumber == pageNumber &&
                    c.PageSize == pageSize &&
                    c.TotalPages == totalPages),
                It.IsAny<CancellationToken>())
                , Times.Once);
        }

        [Test]
        public async Task Then_a_NoContent_response_is_returned()
        {
            // Arrange
            var request = _fixture.Build<JobRequest>().With(r => r.Type, JobType.RefreshLegalEntities).Create();
            var pageNumber = _fixture.Create<long>();
            var pageSize = _fixture.Create<long>();
            var accountLegalEntities = _fixture.CreateMany<AccountLegalEntity>(10);
            var totalPages = _fixture.Create<long>();
            request.Data.Add("PageNumber", pageNumber);
            request.Data.Add("PageSize", pageSize);
            request.Data.Add("TotalPages", totalPages);
            request.Data.Add("AccountLegalEntities", JsonConvert.SerializeObject(accountLegalEntities));

            // Act
            var actual = await _sut.AddJob(request) as NoContentResult;

            // Assert
            actual.Should().NotBeNull();
        }
    }
}
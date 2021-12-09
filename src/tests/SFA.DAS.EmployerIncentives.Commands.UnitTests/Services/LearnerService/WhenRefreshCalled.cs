using AutoFixture;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerServiceTests
{
    public class WhenRefreshCalled
    {
        private LearnerSubmissionService _sut;
        private TestHttpClient _httpClient;
        private Mock<ILogger<LearnerService>> _logger;
        private Uri _baseAddress;
        private Learner _learner;
        private readonly string _version = "1.0";
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));
            _baseAddress = new Uri(@"http://localhost");
            _httpClient = new TestHttpClient(_baseAddress);
            _logger = new Mock<ILogger<LearnerService>>();

            _learner = new LearnerFactory().GetExisting(_fixture.Create<LearnerModel>());

            _sut = new LearnerSubmissionService(_httpClient, _version);
        }

        [Test]
        public async Task Then_the_learner_submission_data_is_null_when_the_learner_data_does_not_exist()
        {
            //Arrange
            _httpClient.SetUpGetAsAsync(System.Net.HttpStatusCode.NotFound);

            //Act
            var response = await _sut.Get(_learner);

            //Assert
            _httpClient.VerifyGetAsAsync($"api/v{_version}/{_learner.Ukprn}/{_learner.UniqueLearnerNumber}?", Times.Once());
            response.Should().BeNull();
        }

        [Test]
        public async Task Then_the_learner_submission_data_is_valid_when_the_learner_data_exists()
        {
            //Arrange
            var learnerSubmissionDto = _fixture.Build<LearnerSubmissionDto>().With(l => l.Ukprn, _learner.Ukprn).Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            var response = await _sut.Get(_learner);

            //Assert
            _httpClient.VerifyGetAsAsync($"api/v{_version}/{_learner.Ukprn}/{_learner.UniqueLearnerNumber}?", Times.Once());
            response.Should().NotBeNull();
            response.RawJson.Should().Be(JsonConvert.SerializeObject(learnerSubmissionDto));
            response.Ukprn.Should().Be(_learner.Ukprn);
        }
    }
}

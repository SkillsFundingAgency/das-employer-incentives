using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Commands.Services.LearnerMatchApi;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.UnitTests.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.Services.LearnerService
{
    //    Conditions for DataLock:
    // 1. ZPROG001 reference
    // 2. PriceEpisode.StartDate <= NexPayment.DueDate => PriceEpisode.EndDate
    // 3. Period.Period == NexPayment.CollectionPeriod
    // 4. Period.ApprenticeshipId == ApprenticeshipIncentive.ApprenticeshipId
    // 5. Period.IsPayable == false
    public class WhenRefreshCalledAndDataLocksAreChecked
    {
        private Commands.Services.LearnerMatchApi.LearnerService _sut;
        private TestHttpClient _httpClient;
        private Uri _baseAddress;
        private Learner _learner;
        private const string Version = "1.0";
        private Fixture _fixture;
        private const short NextPendingPaymentCollectionYear = 2020;
        private const byte NextPendingPaymentCollectionPeriod = 3;
        private readonly DateTime _nextPendingPaymentDueDate = DateTime.Parse($"01/10/{NextPendingPaymentCollectionYear}", new CultureInfo("en-GB"));

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _baseAddress = new Uri(@"http://localhost");
            _httpClient = new TestHttpClient(_baseAddress);

            _learner = new Learner(
                _fixture.Create<Guid>(),
                _fixture.Create<Guid>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<DateTime>());
            _learner.SetNextPendingPayment(new NextPendingPayment(NextPendingPaymentCollectionYear, NextPendingPaymentCollectionPeriod, _nextPendingPaymentDueDate));

            _sut = new Commands.Services.LearnerMatchApi.LearnerService(_httpClient, Version);
        }

        [Test]
        public async Task HasDataLock_is_true_when_learner_is_getting_paid_for_the_same_period_elsewhere()
        {
            //Arrange  
            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.StartDate, DateTime.Parse($"01/09/{NextPendingPaymentCollectionYear}", new CultureInfo("en-GB")))
                            .With(pe => pe.EndDate, DateTime.Parse($"01/12/{NextPendingPaymentCollectionYear}", new CultureInfo("en-GB")))
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                                        _fixture.Build<PeriodDto>()
                                                       .With(period => period.ApprenticeshipId, _learner.ApprenticeshipId)
                                                       .With(period => period.IsPayable, false)
                                                       .With(period => period.Period, NextPendingPaymentCollectionPeriod)
                                                       .Create()
                                                    })
                                                    .Create()
                            })
                        .Create(),
                    _fixture.Create<TrainingDto>()
                })
                .Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.HasDataLock.Should().BeTrue();
        }

        [Test]
        public async Task HasDataLock_is_false_when_learner_is_not_getting_paid_for_the_same_period_elsewhere()
        {
            //Arrange  
            var learnerSubmissionDto = _fixture
                .Build<LearnerSubmissionDto>()
                .With(l => l.Training, new List<TrainingDto> {
                    _fixture.Create<TrainingDto>(),
                    _fixture
                        .Build<TrainingDto>()
                        .With(p => p.Reference, "ZPROG001")
                        .With(p => p.PriceEpisodes, new List<PriceEpisodeDto>(){_fixture.Build<PriceEpisodeDto>()
                            .With(pe => pe.StartDate, DateTime.Parse($"01/09/{NextPendingPaymentCollectionYear}", new CultureInfo("en-GB")))
                            .With(pe => pe.EndDate, DateTime.Parse($"01/12/{NextPendingPaymentCollectionYear}", new CultureInfo("en-GB")))
                            .With(pe => pe.Periods, new List<PeriodDto>(){
                                _fixture.Build<PeriodDto>()
                                    .With(period => period.ApprenticeshipId, _learner.ApprenticeshipId)
                                    .With(period => period.IsPayable, true) // this breaks the data-lock condition !
                                    .With(period => period.Period, NextPendingPaymentCollectionPeriod)
                                    .Create()
                            })
                            .Create()
                            })
                        .Create(),
                    _fixture.Create<TrainingDto>()
                })
                .Create();

            _httpClient.SetUpGetAsAsync(learnerSubmissionDto, System.Net.HttpStatusCode.OK);

            //Act
            await _sut.Refresh(_learner);

            //Assert
            _learner.HasDataLock.Should().BeFalse();
        }
    }
}

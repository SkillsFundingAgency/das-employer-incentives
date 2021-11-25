using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.LearnerTests
{
    public class WhenSetSubmissionData
    {
        private Learner _sut;
        private LearnerModel _sutModel;
        private Fixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _fixture.Customize<LearnerModel>(c => c.Without(x => x.LearningPeriods));
            _sutModel = _fixture
                .Build<LearnerModel>()
                .Without(l => l.LearningPeriods)
                .Without(l => l.SubmissionData)
                .Create();

            _sut = Sut(_sutModel);
        }

        [Test]
        public void Then_new_submission_data_created_when_submission_data_is_null()
        {
            // act
            _sut.SetSubmissionData((SubmissionData)null);

            // assert            
            var model = _sut.GetModel();
            model.SubmissionData.Should().NotBeNull();
        }

        [Test]
        public void Then_submission_data_is_replaced()
        {
            // arrange 
            var expectedSubmissionData = _fixture.Create<SubmissionData>();
            
            // act
            _sut.SetSubmissionData(expectedSubmissionData);

            // assert            
            var model = _sut.GetModel();
            model.SubmissionData.Should().Be(expectedSubmissionData);
        }

        [Test]
        public void Then_an_event_is_raised_when_in_learning()
        {
            // arrange 
            var learningData = new LearningData(true);
            var expectedSubmissionData = _fixture.Create<SubmissionData>();
            expectedSubmissionData.SetLearningData(learningData);

            // act
            _sut.SetSubmissionData(expectedSubmissionData);

            // assert            
            var events = _sut.FlushEvents();
            events.OfType<LearningFound>().ToList().Count.Should().Be(1);
        }

        [Test]
        public void Then_an_event_is_not_raised_when_not_in_learning()
        {
            // arrange 
            var learningData = new LearningData(false);
            var expectedSubmissionData = _fixture.Create<SubmissionData>();
            expectedSubmissionData.SetLearningData(learningData);

            // act
            _sut.SetSubmissionData(expectedSubmissionData);

            // assert            
            var events = _sut.FlushEvents();
            events.OfType<LearningFound>().ToList().Count.Should().Be(0);
        }

        private Learner Sut(LearnerModel model)
        {
            return Learner.Get(model);
        }
    }
}

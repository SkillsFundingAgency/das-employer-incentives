using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.SubmissionDataTests
{
    public class WhenHasChangeOfCircumstances
    {
        private readonly Fixture _fixture = new Fixture();

        [Test]
        public void Then_true_if_learning_periods_changed()
        {
            var data1 = _fixture.Build<ApprenticeshipIncentives.ValueTypes.SubmissionData>()
                .Create();

            var data2 = _fixture.Build<ApprenticeshipIncentives.ValueTypes.SubmissionData>()
                .Create();

            data1.LearningData.SetLearningPeriodsChanged();

            data1.HasChangeOfCircumstances(data2).Should().BeTrue();
        }

        [Test]
        public void Then_false_if_learning_periods_not_changed()
        {
            // Arrange
            var data1 = _fixture.Build<ApprenticeshipIncentives.ValueTypes.SubmissionData>()
                .Create();

            var data2 = _fixture.Build<ApprenticeshipIncentives.ValueTypes.SubmissionData>()
                .Create();

            // Assert
            data1.HasChangeOfCircumstances(data2).Should().BeFalse();
        }
    }
}

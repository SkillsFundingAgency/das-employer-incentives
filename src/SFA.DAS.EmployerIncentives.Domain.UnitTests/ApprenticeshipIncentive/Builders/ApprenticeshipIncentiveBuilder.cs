using AutoFixture;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.UnitTests.ApprenticeshipIncentive.Builders
{
    internal class ApprenticeshipIncentiveBuilder
    {
        private readonly Fixture _fixture;
        private readonly ApprenticeshipIncentiveModel _apprenticeshipIncentiveModel;

        public ApprenticeshipIncentiveBuilder()
        {
            _fixture = new Fixture();

            _apprenticeshipIncentiveModel = _fixture.Build<ApprenticeshipIncentiveModel>().Create();
        }

        public ApprenticeshipIncentiveBuilder WithApprenticeship(Apprenticeship apprenticeship)
        {
            _apprenticeshipIncentiveModel.Apprenticeship = apprenticeship;
            return this;
        }

        public ApprenticeshipIncentiveBuilder WithBreakInLearningDayCount(int breakInLearningDayCount)
        {
            _apprenticeshipIncentiveModel.BreakInLearningDayCount = breakInLearningDayCount;
            return this;
        }

        public ApprenticeshipIncentiveBuilder WithStartDate(DateTime startDate)
        {
            _apprenticeshipIncentiveModel.StartDate = startDate;
            return this;
        }

        public ApprenticeshipIncentives.ApprenticeshipIncentive Build()
        {
            return ApprenticeshipIncentives.ApprenticeshipIncentive.Get(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
        }
    }
}

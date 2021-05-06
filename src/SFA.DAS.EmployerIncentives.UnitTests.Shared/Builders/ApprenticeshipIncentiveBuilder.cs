using AutoFixture;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.Builders
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

        public ApprenticeshipIncentiveBuilder WithIncentivePhase(IncentivePhase incentivePhase)
        {
            _apprenticeshipIncentiveModel.Phase = incentivePhase;
            return this;
        }

        public ApprenticeshipIncentiveBuilder WithStartDate(DateTime startDate)
        {
            _apprenticeshipIncentiveModel.StartDate = startDate;
            return this;
        }

        public ApprenticeshipIncentive Build()
        {
            return ApprenticeshipIncentive.Get(_apprenticeshipIncentiveModel.Id, _apprenticeshipIncentiveModel);
        }
    }
}

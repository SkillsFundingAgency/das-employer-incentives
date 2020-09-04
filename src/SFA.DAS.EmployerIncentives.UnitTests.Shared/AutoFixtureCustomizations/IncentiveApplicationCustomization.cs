using System;
using AutoFixture;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations
{
    public class IncentiveApplicationCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new IncentiveApplicationFactory().CreateNew(fixture.Create<Guid>(), fixture.Create<long>(), fixture.Create<long>()));
            fixture.Register(() => new IncentiveApplicationFactory().CreateApprenticeship(fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), 
                fixture.Create<DateTime>(), fixture.Create<long>(), fixture.Create<DateTime>(), fixture.Create<ApprenticeshipEmployerType>()));
        }
    }

    public class ApprenticeshipIncentiveCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new ApprenticeshipIncentiveFactory().CreateNew(fixture.Create<Guid>(), fixture.Create<Account>(), fixture.Create<Apprenticeship>()));            
        }
    }
}

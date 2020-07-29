using System;
using AutoFixture;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.Commands.UnitTests.AutoFixtureCustomizations
{
    public class IncentiveApplicationCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new IncentiveApplicationFactory().CreateNew(fixture.Create<Guid>(), fixture.Create<long>(), fixture.Create<long>()));
            fixture.Register(() => new IncentiveApplicationFactory().CreateNewApprenticeship(fixture.Create<Guid>(), fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), 
                fixture.Create<DateTime>(), fixture.Create<long>(), fixture.Create<DateTime>(), fixture.Create<ApprenticeshipEmployerType>()));
        }
    }
}

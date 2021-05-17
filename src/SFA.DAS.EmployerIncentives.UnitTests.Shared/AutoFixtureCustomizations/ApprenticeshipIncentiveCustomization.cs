using System;
using AutoFixture;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations
{
    public class ApprenticeshipIncentiveCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new ApprenticeshipIncentiveFactory().CreateNew(
                fixture.Create<Guid>(), 
                fixture.Create<Guid>(), 
                fixture.Create<Account>(), 
                fixture.Create<Apprenticeship>(), 
                fixture.Create<DateTime>(), 
                fixture.Create<DateTime>(), 
                fixture.Create<string>(),
                new  AgreementVersion(fixture.Create<int>()),
                new Domain.ValueObjects.IncentivePhase(Enums.Phase.Phase1)));
        }
    }
}

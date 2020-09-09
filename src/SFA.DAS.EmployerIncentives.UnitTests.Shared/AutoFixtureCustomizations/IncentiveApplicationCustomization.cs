using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.Factories;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.UnitTests.Shared.AutoFixtureCustomizations
{
    public class IncentiveApplicationCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new IncentiveApplicationFactory().CreateNew(fixture.Create<Guid>(), fixture.Create<long>(), fixture.Create<long>()));
            fixture.Register(() => new IncentiveApplicationFactory().CreateApprenticeship(fixture.Create<int>(), fixture.Create<string>(), fixture.Create<string>(), 
                fixture.Create<DateTime>(), fixture.Create<long>(), fixture.Create<DateTime>(), fixture.Create<ApprenticeshipEmployerType>(),
                fixture.Create<List<IncentivePaymentProfile>>()));
        }
    }

    public class ApprenticeshipIncentiveCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new ApprenticeshipIncentiveFactory().CreateNew(fixture.Create<Guid>(), fixture.Create<Account>(), fixture.Create<Apprenticeship>()));            
        }
    }

    public class IncentivePaymentProfileCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new IncentivePaymentProfile(fixture.Create<IncentiveType>(), fixture.CreateMany<PaymentProfile>().ToList()));
        }
    }

    public class PaymentProfileCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register(() => new PaymentProfile(fixture.Create<int>(), fixture.Create<decimal>()));
        }
    }


}

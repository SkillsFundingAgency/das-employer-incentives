using System;

namespace SFA.DAS.EmployerIncentives.Data.UnitTests.TestHelpers
{
    public interface ISqlDatabase : IDisposable
    {
        DatabaseInfo DatabaseInfo { get; }
    }
}

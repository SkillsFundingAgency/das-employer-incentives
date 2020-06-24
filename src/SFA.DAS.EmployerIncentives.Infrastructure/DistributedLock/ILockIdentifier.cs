
namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    public interface ILockIdentifier
    {
        string LockId { get; }
    }
}

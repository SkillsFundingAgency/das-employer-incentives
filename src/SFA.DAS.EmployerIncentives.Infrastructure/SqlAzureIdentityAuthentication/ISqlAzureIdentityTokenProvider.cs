using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    public interface ISqlAzureIdentityTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
        string GetAccessToken(CancellationToken cancellationToken = default);
    }
}

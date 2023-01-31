using Azure.Core;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    public interface IAzureCredential
    {
        public TokenCredential Get(); 
    }
}

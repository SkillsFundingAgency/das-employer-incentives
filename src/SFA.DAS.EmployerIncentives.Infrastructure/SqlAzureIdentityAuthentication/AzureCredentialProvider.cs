using Azure.Core;
using Azure.Identity;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    [ExcludeFromCodeCoverage]
    public class AzureCredentialProvider : IAzureCredential
    {
        public TokenCredential Get()
        {
            return new DefaultAzureCredential();
        }
    }
}

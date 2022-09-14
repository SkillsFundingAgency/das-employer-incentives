using Azure.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private readonly IAzureCredential _azureCredential;

        public SqlAzureIdentityTokenProvider(IAzureCredential azureCredential)
        {
            _azureCredential = azureCredential;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var tokenResponse = await _azureCredential.Get().GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }), cancellationToken);

            return tokenResponse.Token;
        }

        public string GetAccessToken(CancellationToken cancellationToken = default)
        {
            return _azureCredential.Get().GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }), cancellationToken).Token;
        }
    }
}

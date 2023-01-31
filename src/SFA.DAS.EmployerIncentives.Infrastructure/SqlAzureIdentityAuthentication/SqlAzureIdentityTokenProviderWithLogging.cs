using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityTokenProviderWithLogging : ISqlAzureIdentityTokenProvider
    {
        private readonly ILogger<SqlAzureIdentityTokenProvider> _logger;
        private readonly ISqlAzureIdentityTokenProvider _sqlAzureIdentityTokenProvider;

        public SqlAzureIdentityTokenProviderWithLogging(
            ILogger<SqlAzureIdentityTokenProvider> logger,
            ISqlAzureIdentityTokenProvider sqlAzureIdentityTokenProvider)
        {
            _logger = logger;
            _sqlAzureIdentityTokenProvider = sqlAzureIdentityTokenProvider;
        }

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generated SQL AccessToken");

            return _sqlAzureIdentityTokenProvider.GetAccessTokenAsync(cancellationToken);
        }

        public string GetAccessToken(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generated SQL AccessToken");

            return _sqlAzureIdentityTokenProvider.GetAccessToken(cancellationToken);
        }
    }
}

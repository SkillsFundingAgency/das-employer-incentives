using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.SqlAzureIdentityAuthentication
{
    public class SqlAzureIdentityTokenProvider : ISqlAzureIdentityTokenProvider
    {
        private readonly IAzureCredential _azureCredential;
        private readonly IMemoryCache _memoryCache;
        const string CacheKey = "SqlAzureIdentityAuthenticationKey";
        const int CacheDurationSecs = 300;

        public SqlAzureIdentityTokenProvider(
            IAzureCredential azureCredential,
            IMemoryCache memoryCache)
        {
            _azureCredential = azureCredential;
            _memoryCache = memoryCache;
        }

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            return _memoryCache.GetOrCreateAsync(CacheKey, (c) =>
            {
                c.AbsoluteExpiration = DateTime.UtcNow.AddSeconds(CacheDurationSecs);
                return GetTokenASync(cancellationToken);
            });
        }

        public string GetAccessToken(CancellationToken cancellationToken = default)
        {
            return _memoryCache.GetOrCreate(CacheKey, (c) =>
            {
                c.AbsoluteExpiration = DateTime.UtcNow.AddSeconds(300);
                return _azureCredential.Get().GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }), cancellationToken).Token;
            });
        }

        private async Task<string> GetTokenASync(CancellationToken cancellationToken = default)
        {
            var accessToken = await _azureCredential.Get().GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }), cancellationToken);

            return accessToken.Token;
        }
    }
}

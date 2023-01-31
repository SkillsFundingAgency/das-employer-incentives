using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace SFA.DAS.EmployerIncentives.Infrastructure.UnitTests.SqlAzureIdentityTokenProviderTests
{
    public class TestMemoryCache : IMemoryCache
    {
        private string _key;
        private string _value;
        public TestMemoryCache(string key, string value)
        {
            _key = key;
            _value = value;
        }

        public ICacheEntry CreateEntry(object key)
        {
            return new Mock<ICacheEntry>().Object;
        }

        public void Dispose()
        {
            return;
        }

        public void Remove(object key)
        {
            return;
        }

        public bool TryGetValue(object key, out object value)
        {
            if (_key == (string)key)
            {
                value = _value;
                if (value == null)
                {
                    return false;
                }
                return true;
            }
            value = null;
            return false;
        }
    }
}

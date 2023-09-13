using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
#pragma warning disable CA2254 // Template should be a static expression
    [ExcludeFromCodeCoverage]
    public class AzureDistributedLockProvider : IDistributedLockProvider
    {
        private readonly ILogger<AzureDistributedLockProvider> _log;
        private readonly List<ControlledLock> _locks = new();
        private readonly AutoResetEvent _mutex = new(true);
        private readonly BlobContainerClient _container;
        private Timer _renewTimer;
        private static TimeSpan LockTimeout => TimeSpan.FromMinutes(1);
        private static TimeSpan RenewInterval => TimeSpan.FromSeconds(45);

        public AzureDistributedLockProvider(
            IOptions<ApplicationSettings> options,
            ILogger<AzureDistributedLockProvider> log,
            string containerName)
        {
            _log = log;
            _container = new BlobContainerClient(
                options?.Value.DistributedLockStorage, 
                containerName ?? "distributed-locks");
        }

        public async Task<bool> AcquireLock(string Id, CancellationToken cancellationToken)
        {   
            var blob = _container.GetBlobClient(Id);

            if (!await blob.ExistsAsync(cancellationToken))
            {
                try
                {
                    await blob.UploadAsync(new BinaryData(string.Empty), cancellationToken);
                }
                catch (Azure.RequestFailedException ex)
                {
                    if (!ex.Message.StartsWith("There is currently a lease on the blob", StringComparison.OrdinalIgnoreCase))
                    {
                        throw;
                    }
                }
            }

            if (_mutex.WaitOne())
            {
                try
                {
                    var leaseClient = blob.GetBlobLeaseClient();
                    await leaseClient.AcquireAsync(LockTimeout, cancellationToken: cancellationToken);
                    _locks.Add(new ControlledLock(Id, leaseClient));
                    return true;
                }
                catch (Azure.RequestFailedException ex)
                {
                    _log.LogDebug($"Failed to acquire lock {Id} - {ex.Message}");
                    return false;
                }
                finally
                {
                    _mutex.Set();
                }
            }
            return false;
        }

        public async Task ReleaseLock(string Id)
        {
            if (_mutex.WaitOne())
            {
                try
                {
                    var entry = _locks.FirstOrDefault(x => x.Id == Id);

                    if (entry != null)
                    {
                        try
                        {
                            await entry.Client.ReleaseAsync();
                        }
                        catch (Exception ex)
                        {
                            _log.LogError($"Error releasing lock - {ex.Message}");
                        }
                        _locks.Remove(entry);
                    }
                }
                finally
                {
                    _mutex.Set();
                }
            }
        }

        public async Task Start()
        {
            await _container.CreateIfNotExistsAsync();
            _renewTimer = new Timer(async o => await RenewLeases(o), null, RenewInterval, RenewInterval);
        }

        public Task Stop()
        {
            if (_renewTimer != null)
            {
                _renewTimer.Dispose();
                _renewTimer = null;
            }

            return Task.CompletedTask;
        }

        private async Task RenewLeases(object state)
        {
            if (state == null && _mutex.WaitOne())
            {
                try
                {
                    foreach (var entry in _locks)
                    {
                        await RenewLock(entry);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError($"Error renewing leases - {ex.Message}");
                }
                finally
                {
                    _mutex.Set();
                }
            }
        }

        private async Task RenewLock(ControlledLock entry)
        {
            try
            {
                await entry.Client.RenewAsync();
            }
            catch (Exception ex)
            {
                _log.LogError($"Error renewing lease - {ex.Message}");
            }
        }
    }
#pragma warning restore CA2254 // Template should be a static expression
}

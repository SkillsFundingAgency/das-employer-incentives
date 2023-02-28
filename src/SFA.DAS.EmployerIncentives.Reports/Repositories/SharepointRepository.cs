using Microsoft.Extensions.Options;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Respositories
{
    [ExcludeFromCodeCoverage]
    public class SharepointRepository : IReportsRepository
    {
        private readonly HttpClient _client;
        private readonly bool _canSave;

        public SharepointRepository(
            HttpClient client,
            IOptions<ApplicationSettings> options)
        {
            _client = client;
            _canSave = !string.IsNullOrEmpty(options.Value.ReportsConnectionString);
        }

        public async Task Save(ReportsFileInfo fileInfo, Stream stream)
        {
            if (!_canSave)
            {
                return;
            }

            var fileCollectionEndpoint = $"sp.appcontextsite(@target)/web/getfolderbyserverrelativeurl('{fileInfo.Folder}')/files/add(overwrite=true, url='{fileInfo.Name}.{fileInfo.Extension}')?'";

            using var requestContent = new StreamContent(stream);
            stream.Position = 0;
            requestContent.Headers.ContentType = new MediaTypeHeaderValue(fileInfo.ContentType);

            using var response = await _client.PostAsync(fileCollectionEndpoint, requestContent);
            response.EnsureSuccessStatusCode();
        }
    }
}

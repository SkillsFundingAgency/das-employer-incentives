using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Reports.Respositories
{
    public class SharepointRepository : IReportsRepository
    {
        private readonly HttpClient _client;

        public SharepointRepository(HttpClient client)
        {
            _client = client;
        }

        public async Task Save(ReportsFileInfo fileInfo, MemoryStream memoryStream)
        {
            var fileCollectionEndpoint = $"sp.appcontextsite(@target)/web/getfolderbyserverrelativeurl('{fileInfo.Folder}')/files/add(overwrite=true, url='{fileInfo.Name}.{fileInfo.Extension}')?'";

            using var requestContent = new StreamContent(memoryStream);
            memoryStream.Position = 0;
            requestContent.Headers.ContentType = new MediaTypeHeaderValue(fileInfo.ContentType);

            using var response = await _client.PostAsync(fileCollectionEndpoint, requestContent);
            response.EnsureSuccessStatusCode();
        }
    }
}

using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mockaco
{
    public class BinaryResponseBodyStrategy : IResponseBodyStrategy
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BinaryResponseBodyStrategy(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public bool CanHandle(ResponseTemplate responseTemplate)
        {
            responseTemplate.Headers.TryGetValue(HttpHeaders.ContentType, out var contentType);

            return contentType == HttpContentTypes.ApplicationOctetStream || !string.IsNullOrEmpty(responseTemplate.File);
        }

        public async Task<byte[]> GetResponseBodyBytesFromTemplate(ResponseTemplate responseTemplate)
        {
            if (string.IsNullOrEmpty(responseTemplate.File) && string.IsNullOrEmpty(responseTemplate.Body?.ToString()))
            {
                return default;
            }

            if (string.IsNullOrEmpty(responseTemplate.File))
            {
                return Encoding.UTF8.GetBytes(responseTemplate.Body.ToString());
            }

            if (string.IsNullOrEmpty(responseTemplate.Body?.ToString()))
            {
                return await GetFileBytes(responseTemplate.File);
            }

            throw new InvalidOperationException("Both attributes \"body\" and \"file\" are set in the same response template.");
        }

        private async Task<byte[]> GetFileBytes(string path)
        {
            if (path.IsRemoteAbsolutePath())
            {
                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetAsync(path);

                return await response.Content.ReadAsByteArrayAsync();
            }
            else
            {
                return await File.ReadAllBytesAsync(path);
            }
        }
    }
}
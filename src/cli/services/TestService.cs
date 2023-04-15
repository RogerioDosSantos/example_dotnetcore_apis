
using System.Net.Http;
using System.Threading.Tasks;
using donetcore_cli.interfaces;
using Microsoft.Extensions.Logging;

namespace donetcore_cli.services
{
    public class TestService : ITestService
    {
        private readonly ILogger _logger = null;
        private readonly IHttpClientFactory _clientFactory = null;

        public TestService(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
        {
            _logger = loggerFactory.CreateLogger<TestService>();
            _clientFactory = clientFactory;
        }

        public async Task SearchVideo(string videoSearch)
        {
            string search = $"https://www.google.com/search?q=%2Byoutube+{videoSearch}";
            HttpClient client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync(search);
            string ret = await response.Content.ReadAsStringAsync();
            int from = ret.IndexOf("url?q=https://www.youtube.com/channel/") + "url?q=".Length;
            ret = ret.Substring(from, 100);
            int to = ret.LastIndexOf("&amp;sa=");
            ret = ret.Substring(0, to);
            _logger.LogInformation($"URL with video that contain {videoSearch}: {ret}");
        }
    }
}

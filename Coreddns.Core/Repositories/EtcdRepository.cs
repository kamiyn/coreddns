using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coreddns.Core.Repositories
{
    public class EtcdRepository : IEtcdRepostitory
    {
        private readonly IOptions<CoreDdnsOptions> _options;
        private readonly ILogger _logger;
        public EtcdRepository(
            IOptions<CoreDdnsOptions> options
            , ILogger<EtcdRepository> logger)
        {
            _options = options;
            _logger = logger;
        }

        public class EtcdRequest
        {
            public string host { get; set; }
        }

        public async Task SendNewaddrToEtcd(Iddnshost row)
        {
            string url = _options.Value.BaseEtcdUrl + row.name.ToLower();
            var client = new HttpClient();

            // IPv4 登録しない
            // await SendNewaddrToEtcdSub(url + "/v4", row.ipv4, client);
            await SendNewaddrToEtcdSub(url + "/v6", row.ipv6, client);
        }

        private async Task SendNewaddrToEtcdSub(string urlWithSubkey, string ipaddr, HttpClient client)
        {
            if (ipaddr != null)
            {
                var contentStr = Newtonsoft.Json.JsonConvert.SerializeObject(new EtcdRequest
                {
                    host = ipaddr,
                });
                _logger.LogInformation("PUT " + urlWithSubkey + " value=" + contentStr);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("value", contentStr) });
                var response = await client.PutAsync(urlWithSubkey, content);
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(responseStr);
            }
            else
            {
                _logger.LogInformation("DELETE " + urlWithSubkey);
                var response = await client.DeleteAsync(urlWithSubkey);
                var responseStr = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(responseStr);
            }
        }
    }
}

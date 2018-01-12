using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public long? updatedAt { get; set; }
        }

        public class EtcdNode
        {
            public string key { get; set; }
            public string value { get; set; }
        }

        public class EtcdGetResponse
        {
            public string action { get; set; }
            public EtcdNode node { get; set; }
            // public long modifiedIndex { get; set; }
            // public long createdIndex { get; set; }
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
            var now = GetUnixTime();
            var oldEntry = await GetOldEntry(urlWithSubkey, client);

            if (ipaddr != null && oldEntry != null && oldEntry.host == ipaddr && oldEntry.updatedAt > now - 86400)
            {
                _logger.LogInformation("NOT UPDATE " + urlWithSubkey);
                return; // 値の変更がなく、かつ1日以内のレコードであれば更新をしない
            }

            if (ipaddr != null)
            {
                var contentStr = Newtonsoft.Json.JsonConvert.SerializeObject(new EtcdRequest
                {
                    host = ipaddr,
                    updatedAt = now,
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

        private async Task<EtcdRequest> GetOldEntry(string urlWithSubkey, HttpClient client)
        {
            try
            {
                var response = await client.GetAsync(urlWithSubkey);
                var responseStr = await response.Content.ReadAsStringAsync();
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<EtcdGetResponse>(responseStr);
                if (obj != null && obj.node != null)
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<EtcdRequest>(obj.node.value);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return null;
            }
        }

        private static readonly DateTimeOffset epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private static int GetUnixTime()
        {
            return (int)(DateTimeOffset.Now - epoch).TotalSeconds;
        }
    }
}

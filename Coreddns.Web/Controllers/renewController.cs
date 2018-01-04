using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coreddns.Web.Controllers
{
    public class RenewController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<CoreDdnsOptions> _options;
        private readonly DdnsDbContext _context;
        private readonly ILogger _logger;

        public RenewController(IConfiguration configuration
        , IOptions<CoreDdnsOptions> options
        , DdnsDbContext context
        , ILogger<RenewController> logger)
        {
            _configuration = configuration;
            _options = options;
            _context = context;
            _logger = logger;
        }

        private const string okStr = "OK";

        // private readonly static Regex ReQuestion = new Regex(@"ˆ\?", RegexOptions.Compiled | RegexOptions.Singleline);

        [HttpGet("api/renew")]
        public async Task<string> DdnsRenew()
        {
            string q = Request.QueryString.HasValue ? Request.QueryString.Value : "";
            string hostkey = q.StartsWith("?")
            ? q.Substring(1)
            : q;

            var row = _context.ddnshost.FirstOrDefault(x => x.hash == hostkey && x.isvalid);
            if (row == null) return okStr;

            System.Net.IPAddress ip;
            var realipstr = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!System.Net.IPAddress.TryParse(realipstr, out ip))
            {
                ip = Request.HttpContext.Connection.RemoteIpAddress;
            }

            var changed = await RegisterDdnsToDb(ip, row);
            if (changed)
            {
                await SendNewaddrToEtcd(row);
            }
            return okStr;
        }

        public class EtcdRequest
        {
            public string host { get; set; }
        }

        private async Task SendNewaddrToEtcd(ddnshost row)
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

        // 値の変更があったら true を返す
        private async Task<bool> RegisterDdnsToDb(System.Net.IPAddress ip, ddnshost row)
        {
            var now = DateTimeOffset.Now;
            string ipstr = ip.ToString();
            switch (ip.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetwork:
                    if (row.ipv4 == ipstr) return false;
                    row.ipv4 = ipstr;
                    row.updatetimev4 = now;
                    break;
                case System.Net.Sockets.AddressFamily.InterNetworkV6:
                    if (row.ipv6 == ipstr) return false;
                    row.ipv6 = ipstr;
                    row.updatetimev6 = now;
                    break;
            }

            {
                _context.ddnschangelog.Add(new ddnschangelog
                {
                    name = row.name,
                    ip = ipstr,
                    addrfamily = (int)ip.AddressFamily,
                    createtime = now,
                });
            }

            await _context.SaveChangesAsync();

            return true;
        }
    }
}

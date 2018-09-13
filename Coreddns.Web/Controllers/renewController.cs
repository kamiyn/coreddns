using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Coreddns.Web.Controllers
{
    public class RenewController : Controller
    {
        private readonly DdnsDbContext _context;
        private readonly IEtcdRepostitory _etcdRepo;
        private readonly ILogger _logger;

        public RenewController(
        DdnsDbContext context
        , IEtcdRepostitory etcdRepo
        , ILogger<RenewController> logger
        )
        {
            _context = context;
            _etcdRepo = etcdRepo;
            _logger = logger;
        }

        private const string okStr = "OK";

        public class ddnshostParam : Iddnshost
        {
            public string name { get; set; }
            public string ipv4 { get; set; }
            public string ipv6 { get; set; }

            public ddnshostParam(string name, IPAddress ip)
            {
                this.name = name;
                switch (ip.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        ipv4 = ip.ToString();
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        ipv6 = ip.ToString();
                        break;
                }
            }
        }

        [HttpGet("api/renew")]
        public async Task<string> DdnsRenew()
        {
            try
            {
                string q = Request.QueryString.HasValue ? Request.QueryString.Value : "";
                string hostkey = q.StartsWith("?")
                ? q.Substring(1)
                : q;

                var row = await _context.ddnshost.SingleOrDefaultAsync(x => x.hash == hostkey && x.isvalid);
                if (row == null)
                {
                    _logger.LogWarning("Fail");
                    return okStr;
                }

                var ip = GetRealIp();
                await _etcdRepo.SendNewaddrToEtcd(new ddnshostParam(row.name, ip));
                _logger.LogWarning("Success");
                return okStr;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "DdnsRenew Exception");
                throw;
            }
        }

        private IPAddress GetRealIp()
        {
            IPAddress ip;
            if (IPAddress.TryParse(Request.Headers["X-Real-IP"].FirstOrDefault() ?? "", out ip))
            {
                return ip;
            }
            return Request.HttpContext.Connection.RemoteIpAddress;
        }
    }
}

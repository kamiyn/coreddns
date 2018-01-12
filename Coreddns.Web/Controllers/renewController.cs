using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;
using Coreddns.Core.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Coreddns.Web.Controllers
{
    public class RenewController : Controller
    {
        private readonly DdnsDbContext _context;
        private readonly IEtcdRepostitory _etcdRepo;

        public RenewController(
        DdnsDbContext context
        , IEtcdRepostitory etcdRepo)
        {
            _context = context;
            _etcdRepo = etcdRepo;
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
            string q = Request.QueryString.HasValue ? Request.QueryString.Value : "";
            string hostkey = q.StartsWith("?")
            ? q.Substring(1)
            : q;

            var row = await _context.ddnshost.SingleOrDefaultAsync(x => x.hash == hostkey && x.isvalid);
            if (row == null) return okStr;

            var ip = GetRealIp();
            await _etcdRepo.SendNewaddrToEtcd(new ddnshostParam(row.name, ip));
            await WriteLog(row.name, ip);
            return okStr;
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

        // 値の変更があったら true を返す
        private async Task<bool> WriteLog(string name, System.Net.IPAddress ip)
        {
            var now = DateTimeOffset.Now;
            string ipstr = ip.ToString();
            {
                _context.ddnschangelog.Add(new ddnschangelog
                {
                    name = name,
                    ip = ipstr,
                    addrfamily = (int)ip.AddressFamily,
                    createtime = now,
                });
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}

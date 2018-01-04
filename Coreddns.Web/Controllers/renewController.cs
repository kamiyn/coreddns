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
            var changed = await RegisterDdnsToDb(ip, row);
            if (changed)
            {
                await _etcdRepo.SendNewaddrToEtcd(row);
            }
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
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}

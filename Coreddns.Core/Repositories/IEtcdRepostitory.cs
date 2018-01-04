using System.Threading.Tasks;
using Coreddns.Core.Entities.DdnsDb;

namespace Coreddns.Core.Repositories
{
    public interface IEtcdRepostitory
    {
        Task SendNewaddrToEtcd(Iddnshost row);
    }
}

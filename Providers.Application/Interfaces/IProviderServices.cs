
using Providers.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Providers.Application.Interfaces
{
    public interface IProviderService
    {
        Task<Provider> RegisterAsync(Provider entity, int actorId);
        Task<Provider?> GetByIdAsync(int id);
        Task<IEnumerable<Provider>> ListAsync();
        Task UpdateAsync(Provider entity, int actorId);
        Task SoftDeleteAsync(int id, int actorId);
    }
}
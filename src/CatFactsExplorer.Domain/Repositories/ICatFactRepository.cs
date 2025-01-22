using CatFactsExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatFactsExplorer.Domain.Repositories
{
    public interface ICatFactRepository
    {
        Task<List<CatFact>> GetAllAsync();
        Task AddAsync(CatFact catFact);
        Task<CatFact> GetByIdAsync(int id);
        Task UpdateAsync(CatFact catFact);
        Task DeleteAsync(int id);
    }
}

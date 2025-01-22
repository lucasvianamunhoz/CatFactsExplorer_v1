using CatFactsExplorer.Domain.Entities;
using CatFactsExplorer.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CatFactsExplorer.Infrastructure.Data
{
    public class CatFactRepository : ICatFactRepository
    {
        private readonly CatFactsDbContext _dbContext;

        public CatFactRepository(CatFactsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CatFact>> GetAllAsync()
        {
            return await _dbContext.CatFacts.ToListAsync();
        }

        public async Task AddAsync(CatFact catFact)
        {
            await _dbContext.CatFacts.AddAsync(catFact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CatFact> GetByIdAsync(int id)
        {
            return await _dbContext.CatFacts.FindAsync(id);
        }

        public async Task UpdateAsync(CatFact catFact)
        {
            _dbContext.CatFacts.Update(catFact);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var catFact = await _dbContext.CatFacts.FindAsync(id);
            if (catFact != null)
            {
                _dbContext.CatFacts.Remove(catFact);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

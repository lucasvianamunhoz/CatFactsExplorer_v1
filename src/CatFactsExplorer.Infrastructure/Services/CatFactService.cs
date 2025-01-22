using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Models;
using CatFactsExplorer.Domain.Entities;
using CatFactsExplorer.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatFactsExplorer.Infrastructure.Services
{
    public class CatFactService : ICatFactService
    {
        private readonly ICatFactRepository _catFactRepository;

        public CatFactService(ICatFactRepository catFactRepository)
        {
            _catFactRepository = catFactRepository;
        }

        public async Task<List<CatFactDto>> GetAllCatFactsAsync()
        {
 
            var facts = await _catFactRepository.GetAllAsync();

  
            return facts.Select(f => new CatFactDto
            {
                Fact = f.Fact,
                Length = f.Length
            }).ToList();
        }

        public async Task AddCatFactAsync(CatFactDto catFactDto)
        {
 
            var catFact = new CatFact(catFactDto.Fact, catFactDto.Length);

 
            await _catFactRepository.AddAsync(catFact);
        }
    }
}
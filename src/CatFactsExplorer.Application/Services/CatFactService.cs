using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Models;
using CatFactsExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatFactsExplorer.Domain.Repositories;

namespace CatFactsExplorer.Application.Services
{
    public class CatFactService : ICatFactService
    {
        private readonly ICatFactRepository _repository;

        public CatFactService(ICatFactRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CatFactDto>> GetAllCatFactsAsync()
        {
            var facts = await _repository.GetAllAsync();
            return facts.Select(f => new CatFactDto { Fact = f.Fact, Length = f.Length }).ToList();
        }

        public async Task AddCatFactAsync(CatFactDto catFactDto)
        {
            var fact = new CatFact(catFactDto.Fact, catFactDto.Length);
            await _repository.AddAsync(fact);
        }
    }
}

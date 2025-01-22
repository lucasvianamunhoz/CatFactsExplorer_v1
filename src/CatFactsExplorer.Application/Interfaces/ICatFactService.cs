using CatFactsExplorer.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatFactsExplorer.Application.Interfaces
{
    public interface ICatFactService
    {
        Task<List<CatFactDto>> GetAllCatFactsAsync();
        Task AddCatFactAsync(CatFactDto catFactDto);
    }
}

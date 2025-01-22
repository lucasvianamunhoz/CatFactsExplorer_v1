using CatFactsExplorer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CatFactsExplorer.Infrastructure.Data
{
    public class CatFactsDbContext : DbContext
    {
        public CatFactsDbContext(DbContextOptions<CatFactsDbContext> options) : base(options) { }

        public DbSet<CatFact> CatFacts { get; set; }
    }
}

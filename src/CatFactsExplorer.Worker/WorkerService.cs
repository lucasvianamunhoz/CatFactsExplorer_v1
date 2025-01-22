using CatFactsExplorer.Jobs;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatFactsExplorer.Worker
{
    public class WorkerService : BackgroundService
    {
        private readonly IRecurringJobManager _recurringJobManager;

        public WorkerService(IRecurringJobManager recurringJobManager)
        {
            _recurringJobManager = recurringJobManager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Console.WriteLine("Registrando job InsertCatFactsJob...");
                _recurringJobManager.RemoveIfExists("InsertCatFactsJob");
                _recurringJobManager.AddOrUpdate<CatFactUpsertJob>(
                    "InsertCatFactsJob",
                    job => job.RunAsync(),
                    Cron.Hourly);
                Console.WriteLine("Job registrado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar o job: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}

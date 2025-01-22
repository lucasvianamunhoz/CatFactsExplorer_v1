using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Models;

namespace CatFactsExplorer.Jobs
{
    public class CatFactUpsertJob
    {
        private readonly ICatFactService _catFactService;
        private readonly HttpClient _httpClient;

        public CatFactUpsertJob(ICatFactService catFactService, HttpClient httpClient)
        {
            _catFactService = catFactService;
            _httpClient = httpClient;
        }

        public async Task RunAsync()
        {
            try
            {
                const int batchSize = 100;
                const int perPage = 10;
                int pagesToFetch = (batchSize + perPage - 1) / perPage;

                var fetchedFacts = new List<CatFactDto>();

                for (int page = 1; page <= pagesToFetch; page++)
                {
                    var apiResponse = await _httpClient.GetFromJsonAsync<CatFactApiResponse>(
                        $"https://catfact.ninja/facts?limit={perPage}&page={page}");

                    if (apiResponse?.Data != null)
                    {
                        fetchedFacts.AddRange(apiResponse.Data.Select(f => new CatFactDto
                        {
                            Fact = f.Fact,
                            Length = f.Length
                        }));
                    }
                }

                if (!fetchedFacts.Any())
                {
                    Console.WriteLine("No cat facts were returned by the API.");
                    return;
                }

                var existingFacts = await _catFactService.GetAllCatFactsAsync();
                var existingFactSet = new HashSet<string>(existingFacts.Select(f => f.Fact));

                var newFacts = fetchedFacts
                    .Where(f => !existingFactSet.Contains(f.Fact))
                    .ToList();

                if (newFacts.Any())
                {
                    foreach (var fact in newFacts)
                    {
                        await _catFactService.AddCatFactAsync(fact);
                    }
                    Console.WriteLine($"{newFacts.Count} new cat facts added to the database.");
                }
                else
                {
                    Console.WriteLine("No new cat facts to add. All facts are already in the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while executing CatFactUpsertJob: {ex.Message}");
            }
        }

        private class CatFactApiResponse
        {
            public List<CatFactApiModel> Data { get; set; }
        }

        private class CatFactApiModel
        {
            public string Fact { get; set; }
            public int Length { get; set; }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using CatFactsExplorer.Application.Interfaces;
using CatFactsExplorer.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatFactsExplorer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatFactsController : ControllerBase
    {
        private readonly ICatFactService _catFactService;

        public CatFactsController(ICatFactService catFactService)
        {
            _catFactService = catFactService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CatFactDto>>> GetAllCatFacts()
        {
            var catFacts = await _catFactService.GetAllCatFactsAsync();
            return Ok(catFacts);
        }

        [HttpPost]
        public async Task<IActionResult> AddCatFact([FromBody] CatFactDto catFactDto)
        {
            await _catFactService.AddCatFactAsync(catFactDto);
            return CreatedAtAction(nameof(GetAllCatFacts), new { }, catFactDto);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public CategoriasController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CATEGORIAS>>> GetCATEGORIAS()
        {
            return await _context.CATEGORIAS.ToListAsync();
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CATEGORIAS>> GetCATEGORIAS(decimal id)
        {
            var cATEGORIAS = await _context.CATEGORIAS.FindAsync(id);

            if (cATEGORIAS == null)
            {
                return NotFound();
            }

            return cATEGORIAS;
        }

        // PUT: api/Categorias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCATEGORIAS(decimal id, CATEGORIAS cATEGORIAS)
        {
            if (id != cATEGORIAS.ID_CATEGORIA)
            {
                return BadRequest();
            }

            _context.Entry(cATEGORIAS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CATEGORIASExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Categorias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CATEGORIAS>> PostCATEGORIAS(CATEGORIAS cATEGORIAS)
        {
            _context.CATEGORIAS.Add(cATEGORIAS);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CATEGORIASExists(cATEGORIAS.ID_CATEGORIA))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCATEGORIAS", new { id = cATEGORIAS.ID_CATEGORIA }, cATEGORIAS);
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCATEGORIAS(decimal id)
        {
            var cATEGORIAS = await _context.CATEGORIAS.FindAsync(id);
            if (cATEGORIAS == null)
            {
                return NotFound();
            }

            _context.CATEGORIAS.Remove(cATEGORIAS);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CATEGORIASExists(decimal id)
        {
            return _context.CATEGORIAS.Any(e => e.ID_CATEGORIA == id);
        }
    }
}

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
    public class ClientesController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public ClientesController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CLIENTES>>> GetCLIENTES()
        {
            return await _context.CLIENTES.ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CLIENTES>> GetCLIENTES(decimal id)
        {
            var cLIENTES = await _context.CLIENTES.FindAsync(id);

            if (cLIENTES == null)
            {
                return NotFound();
            }

            return cLIENTES;
        }

        // PUT: api/Clientes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCLIENTES(decimal id, CLIENTES cLIENTES)
        {
            if (id != cLIENTES.ID_CLIENTE)
            {
                return BadRequest();
            }

            _context.Entry(cLIENTES).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CLIENTESExists(id))
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

        // POST: api/Clientes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CLIENTES>> PostCLIENTES(CLIENTES cLIENTES)
        {
            _context.CLIENTES.Add(cLIENTES);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CLIENTESExists(cLIENTES.ID_CLIENTE))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCLIENTES", new { id = cLIENTES.ID_CLIENTE }, cLIENTES);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCLIENTES(decimal id)
        {
            var cLIENTES = await _context.CLIENTES.FindAsync(id);
            if (cLIENTES == null)
            {
                return NotFound();
            }

            _context.CLIENTES.Remove(cLIENTES);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CLIENTESExists(decimal id)
        {
            return _context.CLIENTES.Any(e => e.ID_CLIENTE == id);
        }
    }
}

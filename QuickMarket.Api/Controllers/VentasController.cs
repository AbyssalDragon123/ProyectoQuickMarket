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
    public class VentasController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public VentasController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VENTAS>>> GetVENTAS()
        {
            return await _context.VENTAS.ToListAsync();
        }

        // GET: api/Ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VENTAS>> GetVENTAS(decimal id)
        {
            var vENTAS = await _context.VENTAS.FindAsync(id);

            if (vENTAS == null)
            {
                return NotFound();
            }

            return vENTAS;
        }

        // PUT: api/Ventas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVENTAS(decimal id, VENTAS vENTAS)
        {
            if (id != vENTAS.ID_VENTA)
            {
                return BadRequest();
            }

            _context.Entry(vENTAS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VENTASExists(id))
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

        // POST: api/Ventas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<VENTAS>> PostVENTAS(VENTAS vENTAS)
        {
            _context.VENTAS.Add(vENTAS);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VENTASExists(vENTAS.ID_VENTA))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetVENTAS", new { id = vENTAS.ID_VENTA }, vENTAS);
        }

        // DELETE: api/Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVENTAS(decimal id)
        {
            var vENTAS = await _context.VENTAS.FindAsync(id);
            if (vENTAS == null)
            {
                return NotFound();
            }

            _context.VENTAS.Remove(vENTAS);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VENTASExists(decimal id)
        {
            return _context.VENTAS.Any(e => e.ID_VENTA == id);
        }
    }
}

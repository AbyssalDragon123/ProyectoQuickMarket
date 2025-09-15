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
    public class Detalle_VentasController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public Detalle_VentasController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Detalle_Ventas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DETALLE_VENTAS>>> GetDETALLE_VENTAS()
        {
            return await _context.DETALLE_VENTAS.ToListAsync();
        }

        // GET: api/Detalle_Ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DETALLE_VENTAS>> GetDETALLE_VENTAS(decimal id)
        {
            var dETALLE_VENTAS = await _context.DETALLE_VENTAS.FindAsync(id);

            if (dETALLE_VENTAS == null)
            {
                return NotFound();
            }

            return dETALLE_VENTAS;
        }

        // PUT: api/Detalle_Ventas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDETALLE_VENTAS(decimal id, DETALLE_VENTAS dETALLE_VENTAS)
        {
            if (id != dETALLE_VENTAS.ID_DETALLE)
            {
                return BadRequest();
            }

            _context.Entry(dETALLE_VENTAS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DETALLE_VENTASExists(id))
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

        // POST: api/Detalle_Ventas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DETALLE_VENTAS>> PostDETALLE_VENTAS(DETALLE_VENTAS dETALLE_VENTAS)
        {
            _context.DETALLE_VENTAS.Add(dETALLE_VENTAS);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DETALLE_VENTASExists(dETALLE_VENTAS.ID_DETALLE))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetDETALLE_VENTAS", new { id = dETALLE_VENTAS.ID_DETALLE }, dETALLE_VENTAS);
        }

        // DELETE: api/Detalle_Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDETALLE_VENTAS(decimal id)
        {
            var dETALLE_VENTAS = await _context.DETALLE_VENTAS.FindAsync(id);
            if (dETALLE_VENTAS == null)
            {
                return NotFound();
            }

            _context.DETALLE_VENTAS.Remove(dETALLE_VENTAS);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DETALLE_VENTASExists(decimal id)
        {
            return _context.DETALLE_VENTAS.Any(e => e.ID_DETALLE == id);
        }
    }
}

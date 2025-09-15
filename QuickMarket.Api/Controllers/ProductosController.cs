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
    public class ProductosController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public ProductosController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PRODUCTOS>>> GetPRODUCTOS()
        {
            return await _context.PRODUCTOS.ToListAsync();
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PRODUCTOS>> GetPRODUCTOS(decimal id)
        {
            var pRODUCTOS = await _context.PRODUCTOS.FindAsync(id);

            if (pRODUCTOS == null)
            {
                return NotFound();
            }

            return pRODUCTOS;
        }

        // PUT: api/Productos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPRODUCTOS(decimal id, PRODUCTOS pRODUCTOS)
        {
            if (id != pRODUCTOS.ID_PRODUCTO)
            {
                return BadRequest();
            }

            _context.Entry(pRODUCTOS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PRODUCTOSExists(id))
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

        // POST: api/Productos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PRODUCTOS>> PostPRODUCTOS(PRODUCTOS pRODUCTOS)
        {
            _context.PRODUCTOS.Add(pRODUCTOS);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PRODUCTOSExists(pRODUCTOS.ID_PRODUCTO))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPRODUCTOS", new { id = pRODUCTOS.ID_PRODUCTO }, pRODUCTOS);
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePRODUCTOS(decimal id)
        {
            var pRODUCTOS = await _context.PRODUCTOS.FindAsync(id);
            if (pRODUCTOS == null)
            {
                return NotFound();
            }

            _context.PRODUCTOS.Remove(pRODUCTOS);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PRODUCTOSExists(decimal id)
        {
            return _context.PRODUCTOS.Any(e => e.ID_PRODUCTO == id);
        }
    }
}

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
    public class EmpleadosController : ControllerBase
    {
        private readonly QuickMarketContext _context;

        public EmpleadosController(QuickMarketContext context)
        {
            _context = context;
        }

        // GET: api/Empleados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EMPLEADOS>>> GetEMPLEADOS()
        {
            return await _context.EMPLEADOS.ToListAsync();
        }

        // GET: api/Empleados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EMPLEADOS>> GetEMPLEADOS(decimal id)
        {
            var eMPLEADOS = await _context.EMPLEADOS.FindAsync(id);

            if (eMPLEADOS == null)
            {
                return NotFound();
            }

            return eMPLEADOS;
        }

        // PUT: api/Empleados/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEMPLEADOS(decimal id, EMPLEADOS eMPLEADOS)
        {
            if (id != eMPLEADOS.ID_EMPLEADO)
            {
                return BadRequest();
            }

            _context.Entry(eMPLEADOS).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EMPLEADOSExists(id))
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

        // POST: api/Empleados
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EMPLEADOS>> PostEMPLEADOS(EMPLEADOS eMPLEADOS)
        {
            _context.EMPLEADOS.Add(eMPLEADOS);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EMPLEADOSExists(eMPLEADOS.ID_EMPLEADO))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetEMPLEADOS", new { id = eMPLEADOS.ID_EMPLEADO }, eMPLEADOS);
        }

        // DELETE: api/Empleados/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEMPLEADOS(decimal id)
        {
            var eMPLEADOS = await _context.EMPLEADOS.FindAsync(id);
            if (eMPLEADOS == null)
            {
                return NotFound();
            }

            _context.EMPLEADOS.Remove(eMPLEADOS);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EMPLEADOSExists(decimal id)
        {
            return _context.EMPLEADOS.Any(e => e.ID_EMPLEADO == id);
        }
    }
}

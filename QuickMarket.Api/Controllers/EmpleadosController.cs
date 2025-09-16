using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadosController : ControllerBase
    {
        private readonly QuickMarketContext _context;
        public EmpleadosController(QuickMarketContext context) => _context = context;

        private static string Norm(string s) => s.Trim();

        // GET: api/Empleados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmpleadoDto>>> Get()
        {
            var data = await _context.EMPLEADOS
                .AsNoTracking()
                .Select(e => new EmpleadoDto(
                    e.ID_EMPLEADO,
                    e.NOMBRE,
                    e.CARNET,
                    e.CREADO_EN,
                    e.ACTUALIZADO_EN
                ))
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/Empleados/5
        [HttpGet("{id:decimal}")]
        public async Task<ActionResult<EmpleadoDto>> GetById(decimal id)
        {
            var e = await _context.EMPLEADOS
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_EMPLEADO == id);

            if (e is null) return NotFound();

            return new EmpleadoDto(e.ID_EMPLEADO, e.NOMBRE, e.CARNET, e.CREADO_EN, e.ACTUALIZADO_EN);
        }

        // POST: api/Empleados
        [HttpPost]
        public async Task<ActionResult<EmpleadoDto>> Create(EmpleadoCreateDto dto)
        {
            var nombre = Norm(dto.Nombre);
            var carnet = Norm(dto.Carnet);

            var carnetExiste = await _context.EMPLEADOS.AnyAsync(x => x.CARNET == carnet);
            if (carnetExiste) return Conflict("Ya existe un empleado con ese carnet.");

            var entity = new EMPLEADOS
            {
                NOMBRE = nombre,
                CARNET = carnet,
                CREADO_EN = DateTime.UtcNow
            };

            _context.EMPLEADOS.Add(entity);
            await _context.SaveChangesAsync();

            var result = new EmpleadoDto(entity.ID_EMPLEADO, entity.NOMBRE, entity.CARNET, entity.CREADO_EN, entity.ACTUALIZADO_EN);
            return CreatedAtAction(nameof(GetById), new { id = entity.ID_EMPLEADO }, result);
        }

        // PUT: api/Empleados/5
        [HttpPut("{id:decimal}")]
        public async Task<IActionResult> Update(decimal id, EmpleadoUpdateDto dto)
        {
            var entity = await _context.EMPLEADOS.FirstOrDefaultAsync(x => x.ID_EMPLEADO == id);
            if (entity is null) return NotFound();

            var nombre = Norm(dto.Nombre);
            var carnet = Norm(dto.Carnet);

            var carnetDuplicado = await _context.EMPLEADOS
                .AnyAsync(x => x.CARNET == carnet && x.ID_EMPLEADO != id);
            if (carnetDuplicado) return Conflict("Ya existe otro empleado con ese carnet.");

            entity.NOMBRE = nombre;
            entity.CARNET = carnet;
            entity.ACTUALIZADO_EN = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Empleados/5
        [HttpDelete("{id:decimal}")]
        public async Task<IActionResult> Delete(decimal id)
        {
            var entity = await _context.EMPLEADOS.FindAsync(id);
            if (entity is null) return NotFound();

            _context.EMPLEADOS.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

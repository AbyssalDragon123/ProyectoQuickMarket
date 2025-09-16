using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly QuickMarketContext _context;
        public ClientesController(QuickMarketContext context) => _context = context;

        private static string Norm(string s) => s.Trim();

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> Get()
        {
            var data = await _context.CLIENTES
                .AsNoTracking()
                .Select(c => new ClienteDto(
                    c.ID_CLIENTE,
                    c.ID_EMPLEADO,
                    c.NOMBRE,
                    c.TELEFONO
                ))
                .ToListAsync();

            return Ok(data);
        }

        // GET: api/Clientes/5
        [HttpGet("{id:decimal}")]
        public async Task<ActionResult<ClienteDto>> GetById(decimal id)
        {
            var c = await _context.CLIENTES
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_CLIENTE == id);

            if (c is null) return NotFound();

            return new ClienteDto(c.ID_CLIENTE, c.ID_EMPLEADO, c.NOMBRE, c.TELEFONO);
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
        {
            if (dto.IdEmpleado.HasValue)
            {
                var existeEmp = await _context.EMPLEADOS.AnyAsync(e => e.ID_EMPLEADO == dto.IdEmpleado.Value);
                if (!existeEmp) return BadRequest("El empleado asignado no existe.");
            }

            var entity = new CLIENTES
            {
                ID_EMPLEADO = dto.IdEmpleado,
                NOMBRE = Norm(dto.Nombre),
                TELEFONO = string.IsNullOrWhiteSpace(dto.Telefono) ? null : Norm(dto.Telefono)
            };

            _context.CLIENTES.Add(entity);
            await _context.SaveChangesAsync();

            var result = new ClienteDto(entity.ID_CLIENTE, entity.ID_EMPLEADO, entity.NOMBRE, entity.TELEFONO);
            return CreatedAtAction(nameof(GetById), new { id = entity.ID_CLIENTE }, result);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id:decimal}")]
        public async Task<IActionResult> Update(decimal id, ClienteUpdateDto dto)
        {
            var entity = await _context.CLIENTES.FirstOrDefaultAsync(x => x.ID_CLIENTE == id);
            if (entity is null) return NotFound();

            if (dto.IdEmpleado.HasValue)
            {
                var existeEmp = await _context.EMPLEADOS.AnyAsync(e => e.ID_EMPLEADO == dto.IdEmpleado.Value);
                if (!existeEmp) return BadRequest("El empleado asignado no existe.");
            }

            entity.ID_EMPLEADO = dto.IdEmpleado;
            entity.NOMBRE = Norm(dto.Nombre);
            entity.TELEFONO = string.IsNullOrWhiteSpace(dto.Telefono) ? null : Norm(dto.Telefono);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id:decimal}")]
        public async Task<IActionResult> Delete(decimal id)
        {
            var entity = await _context.CLIENTES.FindAsync(id);
            if (entity is null) return NotFound();

            _context.CLIENTES.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

// Controllers/CategoriasController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        public CategoriasController(QuickMarketContext db) => _db = db;

        private static string Norm(string s) => s.Trim().ToUpperInvariant();

        // GET: api/categorias?q=...&page=1&pageSize=20
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAll([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var qry = _db.CATEGORIAS.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = Norm(q);
                qry = qry.Where(c =>
                    c.NOMBRE.ToUpper().Contains(term) ||
                    (c.DESCRIPCION != null && c.DESCRIPCION.ToUpper().Contains(term))
                );
            }

            var total = await qry.CountAsync();
            var items = await qry
                .OrderBy(c => c.NOMBRE)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CategoriaDto(c.ID_CATEGORIA, c.NOMBRE, c.DESCRIPCION, c.CREADO_EN, c.ACTUALIZADO_EN))
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET: api/categorias/5
        [HttpGet("{id:decimal}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoriaDto>> GetById(decimal id)
        {
            var c = await _db.CATEGORIAS.AsNoTracking().FirstOrDefaultAsync(x => x.ID_CATEGORIA == id);
            if (c is null) return NotFound();
            return new CategoriaDto(c.ID_CATEGORIA, c.NOMBRE, c.DESCRIPCION, c.CREADO_EN, c.ACTUALIZADO_EN);
        }

        // POST: api/categorias  (admin)
        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Create([FromBody] CategoriaCreateDto dto)
        {
            var nombre = Norm(dto.Nombre);

            // Unicidad case-insensitive
            var exists = await _db.CATEGORIAS.AsNoTracking()
                .Where(c => c.NOMBRE == nombre)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (exists) return Conflict("Ya existe una categoría con ese nombre.");

            var entity = new CATEGORIAS
            {
                NOMBRE = nombre,
                DESCRIPCION = dto.Descripcion?.Trim(),
                CREADO_EN = DateTime.UtcNow
            };

            _db.CATEGORIAS.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.ID_CATEGORIA },
                new CategoriaDto(entity.ID_CATEGORIA, entity.NOMBRE, entity.DESCRIPCION, entity.CREADO_EN, entity.ACTUALIZADO_EN));
        }

        // PUT: api/categorias/5  (admin)
        [HttpPut("{id:decimal}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Update(decimal id, [FromBody] CategoriaUpdateDto dto)
        {
            var entity = await _db.CATEGORIAS.FirstOrDefaultAsync(c => c.ID_CATEGORIA == id);
            if (entity is null) return NotFound();

            var nombre = Norm(dto.Nombre);

            // Verificar unicidad (excluyendo el propio id)
            var exists = await _db.CATEGORIAS.AsNoTracking()
                .Where(c => c.NOMBRE == nombre && c.ID_CATEGORIA != id)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (exists) return Conflict("Ya existe una categoría con ese nombre.");

            entity.NOMBRE = nombre;
            entity.DESCRIPCION = dto.Descripcion?.Trim();
            entity.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new CategoriaDto(entity.ID_CATEGORIA, entity.NOMBRE, entity.DESCRIPCION, entity.CREADO_EN, entity.ACTUALIZADO_EN));
        }

        // DELETE: api/categorias/5  (admin)
        [HttpDelete("{id:decimal}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Delete(decimal id)
        {
            var entity = await _db.CATEGORIAS.FirstOrDefaultAsync(c => c.ID_CATEGORIA == id);
            if (entity is null) return NotFound();

            _db.CATEGORIAS.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

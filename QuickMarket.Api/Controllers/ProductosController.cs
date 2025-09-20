// Controllers/ProductosController.cs
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
    public class ProductosController : ControllerBase
    {
        private readonly QuickMarketContext _db;
        private readonly ILogger<ProductosController> _log;

        public ProductosController(QuickMarketContext db, ILogger<ProductosController> log)
        {
            _db = db;
            _log = log;
        }

        private static string Norm(string s) => s.Trim().ToUpperInvariant();

        // GET: api/productos?q=...&idCategoria=...&min=...&max=...&page=1&pageSize=20
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetAll(
            [FromQuery] string? q,
            [FromQuery] decimal? idCategoria,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var qry = _db.PRODUCTOS.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = Norm(q);
                qry = qry.Where(p =>
                    p.NOMBRE.ToUpper().Contains(term) ||
                    p.DESCRIPCION.ToUpper().Contains(term)
                );
            }

            if (idCategoria is not null)
                qry = qry.Where(p => p.ID_CATEGORIA == idCategoria);

            if (min is not null)
                qry = qry.Where(p => p.PRECIO_UNITARIO >= min);
            if (max is not null)
                qry = qry.Where(p => p.PRECIO_UNITARIO <= max);

            var total = await qry.CountAsync();
            var items = await qry
                .OrderBy(p => p.NOMBRE)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductoDto(
                    p.ID_PRODUCTO, p.NOMBRE, p.DESCRIPCION, p.PRECIO_UNITARIO, p.STOCK, p.ID_CATEGORIA,
                    p.IVA_UNITARIO, p.PRECIO_CON_IVA
                ))
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        // GET: api/productos/5
        [HttpGet("{id:decimal}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductoDto>> GetById(decimal id)
        {
            var p = await _db.PRODUCTOS.AsNoTracking().FirstOrDefaultAsync(x => x.ID_PRODUCTO == id);
            if (p is null) return NotFound();

            return new ProductoDto(p.ID_PRODUCTO, p.NOMBRE, p.DESCRIPCION, p.PRECIO_UNITARIO, p.STOCK, p.ID_CATEGORIA, p.IVA_UNITARIO, p.PRECIO_CON_IVA);
        }

        // POST: api/productos  (admin)
        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Create([FromBody] ProductoCreateDto dto)
        {
            var nombre = Norm(dto.Nombre);
            var desc = dto.Descripcion?.Trim();

            // Unicidad por nombre dentro de la misma categoría (CI)
            var exists = await _db.PRODUCTOS.AsNoTracking()
                .Where(p => p.ID_CATEGORIA == dto.IdCategoria && p.NOMBRE == nombre)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (exists) return Conflict("Ya existe un producto con ese nombre en la categoría.");

            var entity = new PRODUCTOS
            {
                NOMBRE = nombre,
                DESCRIPCION = desc ?? "",
                PRECIO_UNITARIO = dto.PrecioUnitario,
                STOCK = dto.Stock,
                ID_CATEGORIA = dto.IdCategoria,
                CREADO_EN = DateTime.UtcNow
            };

            _db.PRODUCTOS.Add(entity);
            await _db.SaveChangesAsync();

            // Releer (para que traiga IVA_UNITARIO / PRECIO_CON_IVA si la BD los calcula on insert)
            await _db.Entry(entity).ReloadAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.ID_PRODUCTO },
                new ProductoDto(entity.ID_PRODUCTO, entity.NOMBRE, entity.DESCRIPCION, entity.PRECIO_UNITARIO, entity.STOCK, entity.ID_CATEGORIA, entity.IVA_UNITARIO, entity.PRECIO_CON_IVA));
        }

        // PUT: api/productos/5  (admin)
        [HttpPut("{id:decimal}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Update(decimal id, [FromBody] ProductoUpdateDto dto)
        {
            var entity = await _db.PRODUCTOS.FirstOrDefaultAsync(p => p.ID_PRODUCTO == id);
            if (entity is null) return NotFound();

            var nombre = Norm(dto.Nombre);
            var desc = dto.Descripcion?.Trim();

            var exists = await _db.PRODUCTOS.AsNoTracking()
                .Where(p => p.ID_CATEGORIA == dto.IdCategoria && p.NOMBRE == nombre && p.ID_PRODUCTO != id)
                .Select(_ => 1).FirstOrDefaultAsync() == 1;

            if (exists) return Conflict("Ya existe un producto con ese nombre en la categoría.");

            entity.NOMBRE = nombre;
            entity.DESCRIPCION = desc ?? "";
            entity.PRECIO_UNITARIO = dto.PrecioUnitario;
            entity.STOCK = dto.Stock;
            entity.ID_CATEGORIA = dto.IdCategoria;
            entity.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Releer para obtener IVA/PrecioConIVA recalculados por BD si aplica
            await _db.Entry(entity).ReloadAsync();

            return Ok(new ProductoDto(entity.ID_PRODUCTO, entity.NOMBRE, entity.DESCRIPCION, entity.PRECIO_UNITARIO, entity.STOCK, entity.ID_CATEGORIA, entity.IVA_UNITARIO, entity.PRECIO_CON_IVA));
        }

        // PATCH: api/productos/5/stock  (admin)
        [HttpPatch("{id:decimal}/stock")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> AjustarStock(decimal id, [FromBody] AjusteStockDto dto)
        {
            var entity = await _db.PRODUCTOS.FirstOrDefaultAsync(p => p.ID_PRODUCTO == id);
            if (entity is null) return NotFound();

            var nuevo = entity.STOCK + dto.Delta;
            if (nuevo < 0) return BadRequest("El stock no puede quedar negativo.");

            entity.STOCK = nuevo;
            entity.ACTUALIZADO_EN = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Stock actualizado.", idProducto = id, stock = entity.STOCK });
        }

        // DELETE: api/productos/5  (admin)
        [HttpDelete("{id:decimal}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Delete(decimal id)
        {
            var entity = await _db.PRODUCTOS.FirstOrDefaultAsync(p => p.ID_PRODUCTO == id);
            if (entity is null) return NotFound();

            _db.PRODUCTOS.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

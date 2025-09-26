using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductosService _svc;

        public ProductosController(IProductosService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<ProductoDto>> Create([FromBody] CreateProductoDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdProducto }, created);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProductoDto>> GetById(int id, CancellationToken ct)
        {
            var p = await _svc.GetAsync(id, ct);
            return p is null ? NotFound() : Ok(p);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<ProductoDto>>> List(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            [FromQuery(Name = "categoria")] int? idCategoria = null,
            CancellationToken ct = default)
            => Ok(await _svc.ListAsync(page, pageSize, q, idCategoria, ct));

        [HttpPut("{id:int}")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<ProductoDto>> Update(int id, [FromBody] UpdateProductoDto dto, CancellationToken ct)
            => Ok(await _svc.UpdateAsync(id, dto, ct));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriasService _svc;

        public CategoriasController(ICategoriasService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<CategoriaDto>> Create(CreateCategoriaDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdCategoria }, created);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoriaDto>> GetById(int id, CancellationToken ct)
        {
            var c = await _svc.GetAsync(id, ct);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<CategoriaDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(page, pageSize, q, ct));

        [HttpPut("{id:int}")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<CategoriaDto>> Update(int id, UpdateCategoriaDto dto, CancellationToken ct)
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

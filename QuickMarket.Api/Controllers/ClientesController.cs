using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly IClientesService _svc;
        private readonly ILogger<ClientesController> _log;

        public ClientesController(IClientesService svc, ILogger<ClientesController> log)
        {
            _svc = svc; _log = log;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> Create([FromBody] CreateClienteDto dto, CancellationToken ct)
        {
            var created = await _svc.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdCliente }, created);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> GetById(int id, CancellationToken ct)
        {
            var c = await _svc.GetAsync(id, ct);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpGet("by-user/{idUsuario:int}")]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> GetByUsuario(int idUsuario, CancellationToken ct)
        {
            var c = await _svc.GetByUsuarioAsync(idUsuario, ct);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpGet]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<PagedResult<ClienteDto>>> List(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            CancellationToken ct = default)
            => Ok(await _svc.ListAsync(page, pageSize, q, ct));

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] UpdateClienteDto dto, CancellationToken ct)
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly IVentasService _svc;

        public VentasController(IVentasService svc)
        {
            _svc = svc;
        }

        /// <summary>Crea una venta a partir de una orden (items sin impuestos)</summary>
        [HttpPost]
        [Authorize] // cualquier usuario autenticado puede comprar
        public async Task<ActionResult<object>> CrearVenta([FromBody] CrearOrdenDto orden, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var id = await _svc.CrearVentaAsync(orden, ct);
            return Ok(new { idVenta = id });
        }

        /// <summary>Obtiene una venta con su detalle</summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<VentaConDetalleDto>> ObtenerVenta(int id, CancellationToken ct)
        {
            var v = await _svc.ObtenerVentaAsync(id, ct);
            return v is null ? NotFound() : Ok(v);
        }
    }
}

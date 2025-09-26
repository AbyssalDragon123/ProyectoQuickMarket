using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMarket.Api.Dtos;
using QuickMarket.Api.Services;

namespace QuickMarket.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _svc;
        private readonly ILogger<UsuariosController> _log;

        public UsuariosController(IUsuariosService svc, ILogger<UsuariosController> log)
        {
            _svc = svc;
            _log = log;
        }

        // ===== AUTH =====

        /// <summary>Registro de usuario</summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterUserDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var created = await _svc.RegisterAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.IdUsuario }, created);
        }

        /// <summary>Login (devuelve JWT)</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var token = await _svc.LoginAsync(dto, ct);
            return Ok(new { token });
        }

        /// <summary>Solicita reset de contraseña (envía código de 5 dígitos al correo)</summary>
        [HttpPost("password/reset-request")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetRequest([FromBody] ResetRequestDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            await _svc.RequestPasswordResetAsync(dto, ct);
            return Ok(new { message = "Si el correo existe, se enviará un código para restablecer la contraseña." });
        }

        /// <summary>Completa el reset usando el código de 5 dígitos</summary>
        [HttpPost("password/reset-complete")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetComplete([FromBody] ResetPasswordDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            await _svc.CompletePasswordResetAsync(dto, ct);
            return Ok(new { message = "La contraseña se restableció correctamente." });
        }

        // ===== CRUD =====

        /// <summary>Lista paginada de usuarios (admin)</summary>
        [HttpGet]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<PagedResult<UserDto>>> List(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            CancellationToken ct = default)
        {
            var data = await _svc.ListAsync(page, pageSize, q, ct);
            return Ok(data);
        }

        /// <summary>Obtiene usuario por Id</summary>
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
        {
            var u = await _svc.GetAsync(id, ct);
            return u is null ? NotFound() : Ok(u);
        }

        /// <summary>Actualiza usuario</summary>
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Update(int id, [FromBody] UpdateUserDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var updated = await _svc.UpdateAsync(id, dto, ct);
            return Ok(updated);
        }

        /// <summary>Elimina usuario (admin)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _svc.DeleteAsync(id, ct);
            return NoContent();
        }

        // ❌ Se eliminaron:
        // [GET] api/usuarios/available/username
        // [GET] api/usuarios/available/email
    }
}

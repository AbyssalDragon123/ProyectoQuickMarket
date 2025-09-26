namespace QuickMarket.Api.Dtos
{
    // ======== USUARIOS ========

    // Registro
    public record RegisterUserDto(string Username, string Email, string Password);

    // Login
    public record LoginDto(string UsernameOrEmail, string Password);

    // Usuario completo (lectura)
    public record UserDto(
        int IdUsuario,
        string Username,
        string Email,
        string Rol,
        string Estado,
        DateTime CreadoEn,
        DateTime? ActualizadoEn
    );

    // Actualización
    public record UpdateUserDto(
        string? Username,
        string? Email,
        string? Rol,
        string? Estado,
        string? NewPassword
    );

    // Reset contraseña (solicitud)
    public record ResetRequestDto(string Email);

    // Reset contraseña (completar)
    public record ResetPasswordDto(string Token, string NewPassword);

    // ======== COMÚN ========

    // Resultado paginado genérico
    public class PagedResult<T>
    {
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required int Total { get; init; }
        public required IReadOnlyList<T> Items { get; init; }
    }
}

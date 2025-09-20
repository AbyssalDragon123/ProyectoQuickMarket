// Dtos/AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace QuickMarket.Api.Dtos
{
    public record RegisterDto(
        [Required, MinLength(3), MaxLength(80)] string Username,
        [Required, MinLength(8), MaxLength(128)] string Password,
        [Required, EmailAddress, MaxLength(150)] string Email
    );

    public record LoginDto(
        [Required, MinLength(3), MaxLength(80)] string Username,
        [Required, MinLength(8), MaxLength(128)] string Password
    );

    public record RequestResetDto(
        [Required, EmailAddress, MaxLength(150)] string Email
    );

    public record VerifyCodeDto(
        [Required, EmailAddress, MaxLength(150)] string Email,
        [Required, RegularExpression(@"^\d{5}$")] string Code
    );

    public record ResetPasswordDto(
        [Required, EmailAddress, MaxLength(150)] string Email,
        [Required, RegularExpression(@"^\d{5}$")] string Code,
        [Required, MinLength(8), MaxLength(128)] string NewPassword
    );
}

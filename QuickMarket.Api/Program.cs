using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuickMarket.Api.Data;
using QuickMarket.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// 1) EF Core + Oracle + Logging
// ===============================
builder.Services.AddDbContext<QuickMarketContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle"))
       // Muestra parámetros y valores (útil para diagnosticar)
       .EnableSensitiveDataLogging()
       // Log de SQL y eventos de EF en consola
       .LogTo(Console.WriteLine, LogLevel.Information)
);

// ===============================
// 2) Servicios de aplicación
// ===============================
builder.Services.AddScoped<ICategoriasService, CategoriasService>();
builder.Services.AddScoped<IClientesService, ClientesService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IProductosService, ProductosService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IVentasService, VentasService>();

// ===============================
// 3) Autenticación JWT
// ===============================
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Config faltante: Jwt:Key");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "QuickMarket.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "QuickMarket.Client";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // cambiar a true en producción con HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = key
    };
});

// ===============================
// 4) CORS (Angular local)
// ===============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ===============================
// 5) Controllers + Swagger
// ===============================
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QuickMarket API",
        Version = "v1",
        Description = "API para QuickMarket (Oracle + EF Core)"
    });

    // Seguridad Bearer (JWT)
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Usa: Bearer {tu_token_jwt}"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ===============================
// 6) Pipeline HTTP
// ===============================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

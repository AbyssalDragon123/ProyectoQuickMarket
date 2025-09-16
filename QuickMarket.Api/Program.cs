using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickMarket.Api.Data;          // DbContext
using QuickMarket.Api.Services;      // JwtTokenService, EmailService

var builder = WebApplication.CreateBuilder(args);

// Controllers (si quieres mantener nombres EXACTOS de las props, descomenta la línea)
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // o.PropertyNamingPolicy = null;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext con Oracle
builder.Services.AddDbContext<QuickMarketContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// Servicios de la app
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS para tu Angular (ajusta el origen si corresponde)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurado");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // sin tolerancia de reloj (útil para expiraciones exactas)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger (puedes habilitar también en producción si quieres)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS antes de Auth/Authorization
app.UseCors("AngularClient");

// Orden correcto: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

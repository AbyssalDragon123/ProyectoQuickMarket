using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Data; // <-- DbContext namespace

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // si quieres conservar nombres EXACTOS de propiedades (NOMBRE, ID_EMPLEADO, etc.)
        // o.PropertyNamingPolicy = null;
    });

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext con Oracle (lee appsettings.json -> ConnectionStrings:Oracle)
builder.Services.AddDbContext<QuickMarketContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// CORS para tu Angular (ajusta el origen)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Swagger en Dev (puedes habilitarlo también en Prod si quieres)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS primero que Authorization (recomendado)
app.UseCors("AngularClient");

app.UseAuthorization();

app.MapControllers();

app.Run();

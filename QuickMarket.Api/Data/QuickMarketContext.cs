using System.Threading;
using Microsoft.EntityFrameworkCore;
using QuickMarket.Api.Models;

namespace QuickMarket.Api.Data
{
    public class QuickMarketContext : DbContext
    {
        public QuickMarketContext(DbContextOptions<QuickMarketContext> options) : base(options) { }

        public DbSet<EMPLEADOS> EMPLEADOS { get; set; }
        public DbSet<CLIENTES> CLIENTES { get; set; }
        public DbSet<CATEGORIAS> CATEGORIAS { get; set; }
        public DbSet<PRODUCTOS> PRODUCTOS { get; set; }
        public DbSet<VENTAS> VENTAS { get; set; }
        public DbSet<DETALLE_VENTAS> DETALLE_VENTAS { get; set; }
        public DbSet<LOGIN> LOGIN { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.HasDefaultSchema("QUICKMARKET_USER");

            modelBuilder.Entity<EMPLEADOS>().ToTable("EMPLEADOS");
            modelBuilder.Entity<CLIENTES>().ToTable("CLIENTES");
            modelBuilder.Entity<CATEGORIAS>().ToTable("CATEGORIAS");
            modelBuilder.Entity<PRODUCTOS>().ToTable("PRODUCTOS");
            modelBuilder.Entity<VENTAS>().ToTable("VENTAS");
            modelBuilder.Entity<DETALLE_VENTAS>().ToTable("DETALLE_VENTAS");
            modelBuilder.Entity<LOGIN>().ToTable("LOGIN");

            modelBuilder.Entity<EMPLEADOS>().HasKey(x => x.ID_EMPLEADO);
            modelBuilder.Entity<CLIENTES>().HasKey(x => x.ID_CLIENTE);
            modelBuilder.Entity<CATEGORIAS>().HasKey(x => x.ID_CATEGORIA);
            modelBuilder.Entity<PRODUCTOS>().HasKey(x => x.ID_PRODUCTO);
            modelBuilder.Entity<VENTAS>().HasKey(x => x.ID_VENTA);
            modelBuilder.Entity<DETALLE_VENTAS>().HasKey(x => x.ID_DETALLE);
            modelBuilder.Entity<LOGIN>().HasKey(x => x.ID_LOGIN);

            // EMPLEADOS config + índice único CARNET
            modelBuilder.Entity<EMPLEADOS>(e =>
            {
                e.Property(x => x.NOMBRE).HasColumnType("VARCHAR2(80)").IsRequired();
                e.Property(x => x.CARNET).HasColumnType("VARCHAR2(30)").IsRequired();
                e.HasIndex(x => x.CARNET).IsUnique().HasDatabaseName("UQ_EMPLEADOS_CARNET");
            });

            // Relaciones (FK)
            modelBuilder.Entity<CLIENTES>()
                .HasOne<EMPLEADOS>()
                .WithMany()
                .HasForeignKey(c => c.ID_EMPLEADO)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PRODUCTOS>()
                .HasOne<CATEGORIAS>()
                .WithMany()
                .HasForeignKey(p => p.ID_CATEGORIA)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VENTAS>()
                .HasOne<CLIENTES>()
                .WithMany()
                .HasForeignKey(v => v.ID_CLIENTE)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VENTAS>()
                .HasOne<EMPLEADOS>()
                .WithMany()
                .HasForeignKey(v => v.ID_EMPLEADO)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasOne<VENTAS>()
                .WithMany()
                .HasForeignKey(d => d.ID_VENTA)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasOne<PRODUCTOS>()
                .WithMany()
                .HasForeignKey(d => d.ID_PRODUCTO);

            modelBuilder.Entity<DETALLE_VENTAS>()
                .HasIndex(d => new { d.ID_VENTA, d.ID_PRODUCTO })
                .IsUnique();

            // ===== LOGIN (Oracle) =====
            modelBuilder.Entity<LOGIN>(e =>
            {
                e.HasKey(x => x.ID_LOGIN);

                e.Property(x => x.ID_LOGIN).HasColumnType("NUMBER");
                e.Property(x => x.ID_EMPLEADO).HasColumnType("NUMBER").IsRequired();

                e.Property(x => x.USERNAME).HasColumnType("VARCHAR2(80)").IsRequired();
                e.HasIndex(x => x.USERNAME).IsUnique().HasDatabaseName("UQ_LOGIN_USERNAME");

                e.Property(x => x.GMAIL).HasColumnType("VARCHAR2(150)").IsRequired();
                e.HasIndex(x => x.GMAIL).IsUnique().HasDatabaseName("UQ_LOGIN_EMAIL");

                e.Property(x => x.PASSWORD_HASH).HasColumnType("VARCHAR2(255)").IsRequired();

                e.Property(x => x.RESET_TOKEN).HasColumnType("VARCHAR2(5)");
                e.Property(x => x.RESET_TOKEN_EXP).HasColumnType("TIMESTAMP");

                e.Property(x => x.ESTADO).HasColumnType("VARCHAR2(20)").HasDefaultValue("activo");

                e.Property(x => x.CREADO_EN)
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("SYSTIMESTAMP");

                e.Property(x => x.ACTUALIZADO_EN).HasColumnType("TIMESTAMP");

                // FK explícita a EMPLEADOS
                e.HasOne<EMPLEADOS>()
                 .WithMany()
                 .HasForeignKey(x => x.ID_EMPLEADO)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Normalización blindada (por si alguien persiste fuera de los controladores)
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeLoginStrings();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            NormalizeLoginStrings();
            return base.SaveChanges();
        }

        private void NormalizeLoginStrings()
        {
            foreach (var entry in ChangeTracker.Entries<LOGIN>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Entity.USERNAME))
                        entry.Entity.USERNAME = entry.Entity.USERNAME.Trim().ToUpperInvariant();
                    if (!string.IsNullOrWhiteSpace(entry.Entity.GMAIL))
                        entry.Entity.GMAIL = entry.Entity.GMAIL.Trim().ToUpperInvariant();
                }
            }
        }
    }
}

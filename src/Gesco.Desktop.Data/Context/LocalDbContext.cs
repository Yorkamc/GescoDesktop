using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        // Entidades para gestión offline
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<ClaveActivacion> ClavesActivacion { get; set; }
        public DbSet<SesionLocal> SesionesLocales { get; set; }
        
        // Entidades de actividades
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<ArticuloActividad> ArticulosActividad { get; set; }
        public DbSet<CategoriaArticulo> CategoriasArticulo { get; set; }
        
        // Entidades de ventas
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<TransaccionVenta> TransaccionesVenta { get; set; }
        public DbSet<DetalleTransaccionVenta> DetallesTransaccionVenta { get; set; }

        public LocalDbContext()
        {
        }

        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Gesco",
                    "gesco_local.db"
                );
                
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.Property(e => e.Password).IsRequired();
            });
            
            // Configuración de ClaveActivacion
            modelBuilder.Entity<ClaveActivacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CodigoActivacion).IsUnique();
                entity.Property(e => e.FechaExpiracion).IsRequired();
            });
            
            // Configuración de Actividad
            modelBuilder.Entity<Actividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PrecioEntrada).HasPrecision(10, 2);
                entity.HasMany(e => e.Articulos)
                    .WithOne(e => e.Actividad)
                    .HasForeignKey(e => e.ActividadId);
            });
            
            // Configuración de ArticuloActividad
            modelBuilder.Entity<ArticuloActividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioVenta).HasPrecision(10, 2);
                entity.Property(e => e.CostoActividad).HasPrecision(10, 2);
            });
            
            // Configuración de TransaccionVenta
            modelBuilder.Entity<TransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasPrecision(10, 2);
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);
                entity.Property(e => e.Descuentos).HasPrecision(10, 2);
                entity.HasMany(e => e.Detalles)
                    .WithOne(e => e.Transaccion)
                    .HasForeignKey(e => e.TransaccionId);
            });
            
            // Configuración de DetalleTransaccionVenta
            modelBuilder.Entity<DetalleTransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2);
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);
                entity.Property(e => e.Total).HasPrecision(10, 2);
            });
            
            // Datos semilla para desarrollo
            SeedData(modelBuilder);
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Organización por defecto
            modelBuilder.Entity<Organizacion>().HasData(
                new Organizacion
                {
                    Id = 1,
                    Nombre = "Organización Demo",
                    CorreoContacto = "demo@gesco.com",
                    Activo = true,
                    CreadoEn = DateTime.Now
                }
            );
            
            // Usuario administrador por defecto (password: admin123)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreUsuario = "admin",
                    Correo = "admin@gesco.com",
                    NombreCompleto = "Administrador",
                    Password = "$2a$11$rBNh2aFXK3H8JQhY0z5NXOmL7sPQCHfXOQrpPz0YNhzQHquPHH0Hy", // admin123
                    OrganizacionId = 1,
                    RolId = 1,
                    Activo = true,
                    CreadoEn = DateTime.Now
                }
            );
            
            // Categorías de artículos básicas
            modelBuilder.Entity<CategoriaArticulo>().HasData(
                new CategoriaArticulo { Id = 1, Nombre = "Comida", Activo = true },
                new CategoriaArticulo { Id = 2, Nombre = "Bebidas", Activo = true },
                new CategoriaArticulo { Id = 3, Nombre = "Boletos", Activo = true },
                new CategoriaArticulo { Id = 4, Nombre = "Servicios", Activo = true },
                new CategoriaArticulo { Id = 5, Nombre = "Otros", Activo = true }
            );
        }
    }
}

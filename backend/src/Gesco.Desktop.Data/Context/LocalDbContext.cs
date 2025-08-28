using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        // ============================================
        // DBSETS - TODAS LAS ENTIDADES
        // ============================================
        
        // Sistema Core
        public DbSet<Organizacion> Organizaciones { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Modulo> Modulos { get; set; }

        // Auditoría y Sincronización
        public DbSet<LogAuditoria> LogsAuditoria { get; set; }
        public DbSet<ColaSincronizacion> ColaSincronizacion { get; set; }

        // Licencias y Suscripciones
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<EstadoSuscripcion> EstadosSuscripcion { get; set; }
        public DbSet<Suscripcion> Suscripciones { get; set; }
        public DbSet<ClaveActivacion> ClavesActivacion { get; set; }

        // Actividades
        public DbSet<EstadoActividad> EstadosActividad { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<CategoriaServicio> CategoriasServicio { get; set; }
        public DbSet<ActividadCategoria> ActividadCategorias { get; set; }
        public DbSet<ProductoCategoria> ProductosCategorias { get; set; }

        // Ventas
        public DbSet<EstadoVenta> EstadosVenta { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<TransaccionVenta> TransaccionesVenta { get; set; }
        public DbSet<DetalleTransaccionVenta> DetallesTransaccionesVenta { get; set; }
        public DbSet<PagoTransaccion> PagosTransacciones { get; set; }

        // Combos
        public DbSet<ComboVenta> CombosVenta { get; set; }
        public DbSet<ComboArticulo> ComboArticulos { get; set; }

        // Inventario
        public DbSet<TipoMovimientoInventario> TiposMovimientoInventario { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        // Cierres
        public DbSet<CierreCaja> CierresCaja { get; set; }
        public DbSet<CierreActividad> CierresActividad { get; set; }

        // Sistema
        public DbSet<SecuenciaNumeracion> SecuenciasNumeracion { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<ConfiguracionSistema> ConfiguracionesSistema { get; set; }

        // ============================================
        // CONSTRUCTORES
        // ============================================
        public LocalDbContext() { }
        
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        // ============================================
        // CONFIGURACIÓN DE BASE DE DATOS
        // ============================================
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "gesco_local.db");
                var directory = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        // ============================================
        // CONFIGURACIÓN DEL MODELO
        // ============================================
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // ENTIDADES PRINCIPALES
            // ============================================

            // Organizacion
            modelBuilder.Entity<Organizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CorreoContacto).HasMaxLength(200);
                entity.Property(e => e.TelefonoContacto).HasMaxLength(50);
                entity.Property(e => e.PersonaAdquiriente).HasMaxLength(200);
            });

            // Usuario - SOLO RELACIONES BÁSICAS SIN PROBLEMAS
           modelBuilder.Entity<Usuario>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.NombreUsuario).IsUnique();
    entity.HasIndex(e => e.Correo).IsUnique();
    entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Correo).IsRequired().HasMaxLength(200);
    entity.Property(e => e.NombreCompleto).HasMaxLength(200);
    entity.Property(e => e.Telefono).HasMaxLength(50);
    entity.Property(e => e.Contrasena).IsRequired();

    // Solo las relaciones básicas que YA funcionan
    entity.HasOne(e => e.Organizacion)
        .WithMany(o => o.Usuarios)
        .HasForeignKey(e => e.OrganizacionId)
        .OnDelete(DeleteBehavior.SetNull);

    entity.HasOne(e => e.Rol)
        .WithMany(r => r.Usuarios)
        .HasForeignKey(e => e.RolId)
        .OnDelete(DeleteBehavior.Restrict);
        
    // CreadoPor y ActualizadoPor los dejamos como int? simples por ahora
});

            // Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            // ============================================
            // ENTIDADES DE ESTADOS
            // ============================================

            modelBuilder.Entity<EstadoActividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            modelBuilder.Entity<EstadoVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            modelBuilder.Entity<EstadoSuscripcion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            modelBuilder.Entity<TipoMovimientoInventario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            // ============================================
            // ENTIDADES DE SUSCRIPCIONES
            // ============================================

            modelBuilder.Entity<Membresia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.PrecioMensual).HasPrecision(10, 2);
                entity.Property(e => e.PrecioAnual).HasPrecision(10, 2);
            });

            modelBuilder.Entity<Suscripcion>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Relaciones básicas sin foreign key por ahora
            });

modelBuilder.Entity<ClaveActivacion>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.CodigoActivacion).IsUnique();
    entity.Property(e => e.CodigoActivacion).IsRequired().HasMaxLength(100);
    entity.Property(e => e.LoteGeneracion).HasMaxLength(100);
    entity.Property(e => e.IpActivacion).HasMaxLength(45);
    entity.Property(e => e.RazonRevocacion).HasMaxLength(500);
    entity.Property(e => e.Notas).HasMaxLength(1000);
    
    // Solo relación básica con Suscripción
    entity.HasOne(e => e.Suscripcion)
        .WithMany(s => s.ClavesActivacion)
        .HasForeignKey(e => e.SuscripcionesId)
        .OnDelete(DeleteBehavior.Restrict);
});
            // ============================================
            // ENTIDADES DE ACTIVIDADES
            // ============================================

            modelBuilder.Entity<Actividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.Ubicacion).HasMaxLength(200);
                entity.Property(e => e.HashSync).HasMaxLength(32);
                // Sin relaciones complejas por ahora
            });

            modelBuilder.Entity<CategoriaServicio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
            });

            modelBuilder.Entity<ActividadCategoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Configuración básica sin relaciones
            });

            modelBuilder.Entity<ProductoCategoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.HashSync).HasMaxLength(32);
            });

            // ============================================
            // ENTIDADES DE VENTAS
            // ============================================

            modelBuilder.Entity<Caja>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Ubicacion).HasMaxLength(200);
                entity.Property(e => e.HashSync).HasMaxLength(32);
            });

            modelBuilder.Entity<TransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroTransaccion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroFactura).HasMaxLength(50);
                entity.Property(e => e.Total).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.HashSync).HasMaxLength(32);
            });

            modelBuilder.Entity<DetalleTransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.Total).HasPrecision(10, 2).IsRequired();
            });

            modelBuilder.Entity<PagoTransaccion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.Referencia).HasMaxLength(100);
            });

            // ============================================
            // ENTIDADES DE COMBOS
            // ============================================

            modelBuilder.Entity<ComboVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.PrecioCombo).HasPrecision(10, 2).IsRequired();
            });

            modelBuilder.Entity<ComboArticulo>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // ============================================
            // ENTIDADES DE INVENTARIO
            // ============================================

            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CostoUnitario).HasPrecision(10, 2);
                entity.Property(e => e.ValorTotal).HasPrecision(10, 2);
                entity.Property(e => e.Motivo).HasMaxLength(200);
                entity.Property(e => e.Justificacion).HasMaxLength(500);
            });

            // ============================================
            // ENTIDADES DE CIERRES
            // ============================================

            modelBuilder.Entity<CierreCaja>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MontoVentas).HasPrecision(10, 2);
                entity.Property(e => e.EfectivoCalculado).HasPrecision(10, 2);
                entity.Property(e => e.TarjetasCalculado).HasPrecision(10, 2);
                entity.Property(e => e.SinpesCalculado).HasPrecision(10, 2);
                entity.Property(e => e.EfectivoDeclarado).HasPrecision(10, 2);
                entity.Property(e => e.DiferenciaEfectivo).HasPrecision(10, 2);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.ProblemasReportados).HasMaxLength(1000);
            });

            modelBuilder.Entity<CierreActividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DuracionHoras).HasPrecision(5, 2);
                entity.Property(e => e.TotalVentas).HasPrecision(10, 2);
                entity.Property(e => e.ValorInventarioFinal).HasPrecision(10, 2);
                entity.Property(e => e.ValorMerma).HasPrecision(10, 2);
                entity.Property(e => e.Observaciones).HasMaxLength(1000);
                entity.Property(e => e.ProblemasReportados).HasMaxLength(1000);
            });

            // ============================================
            // ENTIDADES DE SISTEMA
            // ============================================

modelBuilder.Entity<SecuenciaNumeracion>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
    entity.Property(e => e.Prefijo).HasMaxLength(10);

    // Relación con Organizacion
    entity.HasOne(e => e.Organizacion)
        .WithMany(o => o.SecuenciasNumeracion)
        .HasForeignKey(e => e.OrganizacionId)
        .OnDelete(DeleteBehavior.Cascade);

    // Usuario que actualiza
    entity.HasOne(e => e.ActualizadoPorUsuario)
        .WithMany()
        .HasForeignKey(e => e.ActualizadoPor)
        .OnDelete(DeleteBehavior.SetNull);
});

modelBuilder.Entity<Notificacion>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Mensaje).IsRequired();
    entity.Property(e => e.DatosAdicionales).HasMaxLength(2000);
    entity.Property(e => e.CanalesEntrega).HasMaxLength(100);
    entity.Property(e => e.CreadaEn).IsRequired();

    // Relación con Organizacion
    entity.HasOne(e => e.Organizacion)
        .WithMany(o => o.Notificaciones)
        .HasForeignKey(e => e.OrganizacionId)
        .OnDelete(DeleteBehavior.Restrict);
    /*
    // Relación Usuario destinatario (UsuarioId -> Usuario)
    entity.HasOne(e => e.Usuario)
        .WithMany() // Sin colección de navegación inversa para evitar conflictos
        .HasForeignKey(e => e.UsuarioId)
        .OnDelete(DeleteBehavior.SetNull);

    // Relación Usuario creador (CreadaPor -> CreadaPorUsuario)  
    entity.HasOne(e => e.CreadaPorUsuario)
        .WithMany() // Sin colección de navegación inversa
        .HasForeignKey(e => e.CreadaPor)
        .OnDelete(DeleteBehavior.SetNull);
        */
});
modelBuilder.Entity<ConfiguracionSistema>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Clave).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Valor).IsRequired();
    entity.Property(e => e.TipoValor).IsRequired().HasMaxLength(50);
    entity.Property(e => e.Categoria).HasMaxLength(100);
    entity.Property(e => e.Descripcion).HasMaxLength(500);
    entity.Property(e => e.ValorPorDefecto).HasMaxLength(1000);
    entity.Property(e => e.NivelAcceso).HasMaxLength(50).HasDefaultValue("admin");
    
    // Solo relación básica con Organización
    entity.HasOne(e => e.Organizacion)
        .WithMany(o => o.ConfiguracionesSistema)
        .HasForeignKey(e => e.OrganizacionId)
        .OnDelete(DeleteBehavior.Cascade);
});
            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tabla).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Accion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Modulo).HasMaxLength(100);
                entity.Property(e => e.DatosAnteriores).HasMaxLength(4000);
                entity.Property(e => e.DatosNuevos).HasMaxLength(4000);
            });

            modelBuilder.Entity<ColaSincronizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TablaAfectada).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Operacion).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DatosCambio).IsRequired();
            });

            modelBuilder.Entity<Modulo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            // ============================================
            // DATOS SEMILLA
            // ============================================
            SeedData(modelBuilder);
        }

        // ============================================
        // MÉTODO DE DATOS SEMILLA
        // ============================================
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Organización por defecto
            modelBuilder.Entity<Organizacion>().HasData(
                new Organizacion
                {
                    Id = 1,
                    Nombre = "Organización Demo",
                    CorreoContacto = "demo@gesco.com",
                    TelefonoContacto = "2222-2222",
                    Direccion = "San José, Costa Rica",
                    PersonaAdquiriente = "Administrador Demo",
                    Activo = true,
                    CreadoEn = DateTime.Now
                }
            );

            // Roles del sistema
            modelBuilder.Entity<Rol>().HasData(
                new Rol 
                { 
                    Id = 1, 
                    Nombre = "Administrador", 
                    Descripcion = "Acceso completo al sistema", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new Rol 
                { 
                    Id = 2, 
                    Nombre = "Vendedor", 
                    Descripcion = "Acceso a ventas y caja", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new Rol 
                { 
                    Id = 3, 
                    Nombre = "Supervisor", 
                    Descripcion = "Supervisión de actividades", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                }
            );

            // Usuario administrador por defecto
            // Password: admin123 (hash BCrypt)
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    NombreUsuario = "admin",
                    Correo = "admin@gesco.com",
                    NombreCompleto = "Administrador del Sistema",
                    Telefono = "8888-8888",
                    Contrasena = "$2a$11$rBNh2aFXK3H8JQhY0z5NXOmL7sPQCHfXOQrpPz0YNhzQHquPHH0Hy", // admin123
                    OrganizacionId = 1,
                    RolId = 1,
                    Activo = true,
                    PrimerLogin = true,
                    CreadoEn = DateTime.Now
                }
            );

            // Estados de actividades
            modelBuilder.Entity<EstadoActividad>().HasData(
                new EstadoActividad 
                { 
                    Id = 1, 
                    Nombre = "Sin Iniciar", 
                    Descripcion = "Actividad no iniciada", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new EstadoActividad 
                { 
                    Id = 2, 
                    Nombre = "En Curso", 
                    Descripcion = "Actividad en desarrollo", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new EstadoActividad 
                { 
                    Id = 3, 
                    Nombre = "Terminada", 
                    Descripcion = "Actividad completada", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new EstadoActividad 
                { 
                    Id = 4, 
                    Nombre = "Cancelada", 
                    Descripcion = "Actividad cancelada", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                }
            );

            // Estados de ventas
            modelBuilder.Entity<EstadoVenta>().HasData(
                new EstadoVenta 
                { 
                    Id = 1, 
                    Nombre = "Pendiente", 
                    Descripcion = "Venta pendiente de procesamiento", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new EstadoVenta 
                { 
                    Id = 2, 
                    Nombre = "Completada", 
                    Descripcion = "Venta completada exitosamente", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new EstadoVenta 
                { 
                    Id = 3, 
                    Nombre = "Cancelada", 
                    Descripcion = "Venta cancelada", 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                }
            );

            // Métodos de pago
            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago 
                { 
                    Id = 1, 
                    Nombre = "Efectivo", 
                    Descripcion = "Pago en efectivo", 
                    RequiereReferencia = false, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new MetodoPago 
                { 
                    Id = 2, 
                    Nombre = "Tarjeta", 
                    Descripcion = "Pago con tarjeta de crédito/débito", 
                    RequiereReferencia = true, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new MetodoPago 
                { 
                    Id = 3, 
                    Nombre = "SINPE Móvil", 
                    Descripcion = "Pago con SINPE Móvil", 
                    RequiereReferencia = true, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                }
            );

            // Tipos de movimiento de inventario
            modelBuilder.Entity<TipoMovimientoInventario>().HasData(
                new TipoMovimientoInventario 
                { 
                    Id = 1, 
                    Nombre = "Entrada", 
                    Descripcion = "Entrada de mercancía al inventario", 
                    AfectaStock = true, 
                    RequiereJustificacion = false, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new TipoMovimientoInventario 
                { 
                    Id = 2, 
                    Nombre = "Venta", 
                    Descripcion = "Salida por venta de productos", 
                    AfectaStock = true, 
                    RequiereJustificacion = false, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                },
                new TipoMovimientoInventario 
                { 
                    Id = 3, 
                    Nombre = "Ajuste", 
                    Descripcion = "Ajuste de inventario por diferencias", 
                    AfectaStock = true, 
                    RequiereJustificacion = true, 
                    Activo = true, 
                    CreadoEn = DateTime.Now 
                }
            );

            // Estados de suscripción
            modelBuilder.Entity<EstadoSuscripcion>().HasData(
                new EstadoSuscripcion 
                { 
                    Id = 1, 
                    Nombre = "Activa", 
                    Descripcion = "Suscripción activa", 
                    PermiteUsoSistema = true, 
                    Activo = true 
                },
                new EstadoSuscripcion 
                { 
                    Id = 2, 
                    Nombre = "Suspendida", 
                    Descripcion = "Suscripción suspendida", 
                    PermiteUsoSistema = false, 
                    Activo = true 
                },
                new EstadoSuscripcion 
                { 
                    Id = 3, 
                    Nombre = "Cancelada", 
                    Descripcion = "Suscripción cancelada", 
                    PermiteUsoSistema = false, 
                    Activo = true 
                }
            );
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Gesco.Desktop.Data.Entities;
using System;
using System.IO;

namespace Gesco.Desktop.Data.Context
{
    public class LocalDbContext : DbContext
    {
        // ============================================
        // ENTIDADES PRINCIPALES
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
                // Para desarrollo - guardar en la carpeta del proyecto
                var dbPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "data",
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

            // ============================================
            // CONFIGURACIONES DE ENTIDADES
            // ============================================

            // Organización
            modelBuilder.Entity<Organizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Activo, e.Nombre });
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CorreoContacto).HasMaxLength(200);
                entity.Property(e => e.TelefonoContacto).HasMaxLength(50);
                entity.Property(e => e.PersonaAdquiriente).HasMaxLength(200);
            });

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Correo).IsUnique();
                entity.HasIndex(e => new { e.OrganizacionId, e.RolId, e.Activo });
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Correo).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NombreCompleto).HasMaxLength(200);
                entity.Property(e => e.Telefono).HasMaxLength(50);
                entity.Property(e => e.Contrasena).IsRequired();

                // Relaciones
                entity.HasOne(e => e.Organizacion)
                    .WithMany(o => o.Usuarios)
                    .HasForeignKey(e => e.OrganizacionId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Rol)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(e => e.RolId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Activo);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Actividad
            modelBuilder.Entity<Actividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.OrganizacionId, e.EstadoId, e.FechaInicio });
                entity.HasIndex(e => new { e.FechaInicio, e.FechaFin, e.OrganizacionId });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Ubicacion).HasMaxLength(200);
                entity.Property(e => e.HashSync).HasMaxLength(32);

                // Relaciones
                entity.HasOne(e => e.Estado)
                    .WithMany(es => es.Actividades)
                    .HasForeignKey(e => e.EstadoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Organizacion)
                    .WithMany(o => o.Actividades)
                    .HasForeignKey(e => e.OrganizacionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CategoriaServicio
            modelBuilder.Entity<CategoriaServicio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.OrganizacionId, e.Activo });
                entity.HasIndex(e => new { e.OrganizacionId, e.Nombre }).IsUnique();
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            });

            // ProductoCategoria
            modelBuilder.Entity<ProductoCategoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ActividadCategoriaId, e.Activo });
                entity.HasIndex(e => new { e.ActividadCategoriaId, e.Codigo }).IsUnique();
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.HashSync).HasMaxLength(32);

                // Constraint para stock positivo
                entity.HasCheckConstraint("CK_ProductoCategoria_CantidadActual", "[CantidadActual] >= 0");
                entity.HasCheckConstraint("CK_ProductoCategoria_PrecioUnitario", "[PrecioUnitario] > 0");
            });

            // Caja
            modelBuilder.Entity<Caja>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ActividadId, e.NumeroCaja }).IsUnique();
                entity.HasIndex(e => new { e.ActividadId, e.Abierta });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Nombre).HasMaxLength(100);
                entity.Property(e => e.Ubicacion).HasMaxLength(200);
                entity.Property(e => e.HashSync).HasMaxLength(32);

                // Constraint para número de caja positivo
                entity.HasCheckConstraint("CK_Caja_NumeroCaja", "[NumeroCaja] > 0");
            });

            // TransaccionVenta
            modelBuilder.Entity<TransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CajaId, e.FechaTransaccion, e.EstadoId });
                entity.HasIndex(e => new { e.NumeroTransaccion, e.CajaId }).IsUnique();
                entity.HasIndex(e => e.NumeroFactura).IsUnique();
                entity.HasIndex(e => new { e.VendedorUsuarioId, e.FechaTransaccion });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.NumeroTransaccion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.NumeroFactura).HasMaxLength(50);
                entity.Property(e => e.Total).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.HashSync).HasMaxLength(32);

                // Constraint para total positivo
                entity.HasCheckConstraint("CK_TransaccionVenta_Total", "[Total] > 0");
            });

            // DetalleTransaccionVenta
            modelBuilder.Entity<DetalleTransaccionVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransaccionId);
                entity.HasIndex(e => e.ArticuloId);
                entity.HasIndex(e => e.ComboId);
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.Total).HasPrecision(10, 2).IsRequired();

                // Constraints
                entity.HasCheckConstraint("CK_DetalleTransaccion_Cantidad", "[Cantidad] > 0");
                entity.HasCheckConstraint("CK_DetalleTransaccion_PrecioUnitario", "[PrecioUnitario] > 0");
                entity.HasCheckConstraint("CK_DetalleTransaccion_ArticuloOrCombo", 
                    "([ArticuloId] IS NOT NULL AND [ComboId] IS NULL AND [EsCombo] = 0) OR ([ArticuloId] IS NULL AND [ComboId] IS NOT NULL AND [EsCombo] = 1)");
            });

            // PagoTransaccion
            modelBuilder.Entity<PagoTransaccion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TransaccionId);
                entity.HasIndex(e => e.MetodoPagoId);
                entity.HasIndex(e => e.Referencia);
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Monto).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.Referencia).HasMaxLength(100);

                // Constraint para monto positivo
                entity.HasCheckConstraint("CK_PagoTransaccion_Monto", "[Monto] > 0");
            });

            // ComboVenta
            modelBuilder.Entity<ComboVenta>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ActividadId, e.Activo });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PrecioCombo).HasPrecision(10, 2).IsRequired();

                // Constraint para precio positivo
                entity.HasCheckConstraint("CK_ComboVenta_PrecioCombo", "[PrecioCombo] > 0");
            });

            // ComboArticulo
            modelBuilder.Entity<ComboArticulo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ComboId, e.ArticuloId }).IsUnique();
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });

                // Constraint para cantidad positiva
                entity.HasCheckConstraint("CK_ComboArticulo_Cantidad", "[Cantidad] > 0");
            });

            // MovimientoInventario
            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ArticuloId, e.FechaMovimiento });
                entity.HasIndex(e => new { e.TipoMovimientoId, e.FechaMovimiento });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.CostoUnitario).HasPrecision(10, 2);
                entity.Property(e => e.ValorTotal).HasPrecision(10, 2);

                // Constraint para cantidad coherente
                entity.HasCheckConstraint("CK_MovimientoInventario_Cantidad", "[Cantidad] != 0");
                entity.HasCheckConstraint("CK_MovimientoInventario_CantidadCoherente", 
                    "[CantidadPosterior] = [CantidadAnterior] + [Cantidad]");
            });

            // CierreCaja
            modelBuilder.Entity<CierreCaja>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CajaId, e.FechaCierre });
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.MontoVentas).HasPrecision(10, 2);
                entity.Property(e => e.EfectivoCalculado).HasPrecision(10, 2);
                entity.Property(e => e.TarjetasCalculado).HasPrecision(10, 2);
                entity.Property(e => e.SinpesCalculado).HasPrecision(10, 2);
                entity.Property(e => e.EfectivoDeclarado).HasPrecision(10, 2);
                entity.Property(e => e.DiferenciaEfectivo).HasPrecision(10, 2);

                // Constraint para fechas coherentes
                entity.HasCheckConstraint("CK_CierreCaja_Fechas", "[FechaCierre] > [FechaApertura]");
            });

            // CierreActividad
            modelBuilder.Entity<CierreActividad>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ActividadId).IsUnique();
                entity.HasIndex(e => new { e.Sincronizado, e.FechaUltimaSync });
                entity.Property(e => e.DuracionHoras).HasPrecision(5, 2);
                entity.Property(e => e.TotalVentas).HasPrecision(10, 2);
                entity.Property(e => e.ValorInventarioFinal).HasPrecision(10, 2);
                entity.Property(e => e.ValorMerma).HasPrecision(10, 2);
            });

            // SecuenciaNumeracion
            modelBuilder.Entity<SecuenciaNumeracion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.OrganizacionId, e.Tipo }).IsUnique();
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Prefijo).HasMaxLength(10);
            });

            // LogAuditoria
            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UsuarioId, e.Fecha });
                entity.HasIndex(e => new { e.Tabla, e.RegistroId, e.Fecha });
                entity.HasIndex(e => new { e.OrganizacionId, e.Fecha });
                entity.Property(e => e.Tabla).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Accion).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Modulo).HasMaxLength(100);
            });

            // ColaSincronizacion
            modelBuilder.Entity<ColaSincronizacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.OrganizacionId, e.Procesado, e.Prioridad });
                entity.HasIndex(e => new { e.TablaAfectada, e.RegistroId });
                entity.Property(e => e.TablaAfectada).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Operacion).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DatosCambio).IsRequired();
            });

            // Configuraciones adicionales...
            ConfigurarEntidadesSuscripciones(modelBuilder);
            ConfigurarNotificacionesYConfiguracion(modelBuilder);

            // Datos semilla para desarrollo
            SeedData(modelBuilder);
        }

        private void ConfigurarEntidadesSuscripciones(ModelBuilder modelBuilder)
        {
            // Membresia
            modelBuilder.Entity<Membresia>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Activo, e.PrecioMensual });
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PrecioMensual).HasPrecision(10, 2);
                entity.Property(e => e.PrecioAnual).HasPrecision(10, 2);
            });

            // EstadoSuscripcion
            modelBuilder.Entity<EstadoSuscripcion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            });

            // Suscripcion
            modelBuilder.Entity<Suscripcion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.OrganizacionId, e.EstadoId });
                entity.HasIndex(e => e.FechaVencimiento);
                entity.HasIndex(e => e.FechaFinGracia);
            });

            // ClaveActivacion
            modelBuilder.Entity<ClaveActivacion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CodigoActivacion).IsUnique();
                entity.HasIndex(e => new { e.SuscripcionesId, e.Utilizada, e.Expirada });
                entity.Property(e => e.CodigoActivacion).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LoteGeneracion).HasMaxLength(100);
                entity.Property(e => e.IpActivacion).HasMaxLength(45);
            });
        }

private void ConfigurarNotificacionesYConfiguracion(ModelBuilder modelBuilder)
{
    // Notificacion - CONFIGURACION CORREGIDA
    modelBuilder.Entity<Notificacion>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.OrganizacionId, e.Leida, e.Importante });
        entity.HasIndex(e => new { e.UsuarioId, e.Leida, e.FechaProgramada });
        entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Mensaje).IsRequired();
        entity.Property(e => e.CanalesEntrega).HasMaxLength(100);

        // FIX CRITICO: Configurar relaciones explicitamente para evitar ambiguedad
        entity.HasOne(e => e.Organizacion)
            .WithMany(o => o.Notificaciones)
            .HasForeignKey(e => e.OrganizacionId)
            .OnDelete(DeleteBehavior.Cascade);

        // RELACION 1: Usuario destinatario (UsuarioId)
        entity.HasOne(e => e.Usuario)
            .WithMany() // SIN PARAMETROS - esto evita la ambiguedad
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        // RELACION 2: Usuario creador (CreadaPor)
        entity.HasOne(e => e.CreadaPorUsuario)
            .WithMany() // SIN PARAMETROS - esto evita la ambiguedad
            .HasForeignKey(e => e.CreadaPor)
            .OnDelete(DeleteBehavior.SetNull);
    });

    // ConfiguracionSistema
    modelBuilder.Entity<ConfiguracionSistema>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.OrganizacionId, e.Clave }).IsUnique();
        entity.HasIndex(e => new { e.Clave, e.Categoria });
        entity.Property(e => e.Clave).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Valor).IsRequired();
        entity.Property(e => e.TipoValor).IsRequired().HasMaxLength(50);
        entity.Property(e => e.Categoria).HasMaxLength(100);
        entity.Property(e => e.NivelAcceso).HasMaxLength(50).HasDefaultValue("admin");

        // Relacion con Usuario para ActualizadoPor
        entity.HasOne(e => e.ActualizadoPorUsuario)
            .WithMany() // SIN PARAMETROS
            .HasForeignKey(e => e.ActualizadoPor)
            .OnDelete(DeleteBehavior.SetNull);
    });
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
                    TelefonoContacto = "2222-2222",
                    Direccion = "San José, Costa Rica",
                    PersonaAdquiriente = "Administrador Demo",
                    Activo = true,
                    CreadoEn = DateTime.Now
                }
            );

            // Rol administrador
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

            // Usuario administrador por defecto (password: admin123)
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
                    CreadoEn = DateTime.Now
                }
            );

            // Estados básicos
            modelBuilder.Entity<EstadoActividad>().HasData(
                new EstadoActividad { Id = 1, Nombre = "Sin Iniciar", Descripcion = "Actividad no iniciada", Activo = true, CreadoEn = DateTime.Now },
                new EstadoActividad { Id = 2, Nombre = "En Curso", Descripcion = "Actividad en desarrollo", Activo = true, CreadoEn = DateTime.Now },
                new EstadoActividad { Id = 3, Nombre = "Terminada", Descripcion = "Actividad completada", Activo = true, CreadoEn = DateTime.Now },
                new EstadoActividad { Id = 4, Nombre = "Cancelada", Descripcion = "Actividad cancelada", Activo = true, CreadoEn = DateTime.Now }
            );

            modelBuilder.Entity<EstadoVenta>().HasData(
                new EstadoVenta { Id = 1, Nombre = "Pendiente", Descripcion = "Venta pendiente", Activo = true, CreadoEn = DateTime.Now },
                new EstadoVenta { Id = 2, Nombre = "Completada", Descripcion = "Venta completada", Activo = true, CreadoEn = DateTime.Now },
                new EstadoVenta { Id = 3, Nombre = "Cancelada", Descripcion = "Venta cancelada", Activo = true, CreadoEn = DateTime.Now }
            );

            modelBuilder.Entity<MetodoPago>().HasData(
                new MetodoPago { Id = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo", RequiereReferencia = false, Activo = true, CreadoEn = DateTime.Now },
                new MetodoPago { Id = 2, Nombre = "Tarjeta", Descripcion = "Pago con tarjeta", RequiereReferencia = true, Activo = true, CreadoEn = DateTime.Now },
                new MetodoPago { Id = 3, Nombre = "SINPE Móvil", Descripcion = "Pago con SINPE", RequiereReferencia = true, Activo = true, CreadoEn = DateTime.Now }
            );

            modelBuilder.Entity<TipoMovimientoInventario>().HasData(
                new TipoMovimientoInventario { Id = 1, Nombre = "Entrada", Descripcion = "Entrada de mercancía", AfectaStock = true, RequiereJustificacion = false, Activo = true, CreadoEn = DateTime.Now },
                new TipoMovimientoInventario { Id = 2, Nombre = "Venta", Descripcion = "Salida por venta", AfectaStock = true, RequiereJustificacion = false, Activo = true, CreadoEn = DateTime.Now },
                new TipoMovimientoInventario { Id = 3, Nombre = "Ajuste", Descripcion = "Ajuste de inventario", AfectaStock = true, RequiereJustificacion = true, Activo = true, CreadoEn = DateTime.Now }
            );
        }
    }
}
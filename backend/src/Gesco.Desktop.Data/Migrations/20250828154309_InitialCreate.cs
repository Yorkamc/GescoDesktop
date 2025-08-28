using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gesco.Desktop.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadosActividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosActividad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosSuscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PermiteUsoSistema = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosSuscripcion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosVenta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Membresias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrecioMensual = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    PrecioAnual = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    LimiteUsuarios = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membresias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RequiereReferencia = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodosPago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CorreoContacto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TelefonoContacto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: false),
                    PersonaAdquiriente = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposMovimientoInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AfectaStock = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiereJustificacion = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMovimientoInventario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Actividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HoraFin = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    Ubicacion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    EstadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EncargadoUsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashSync = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actividades_EstadosActividad_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosActividad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actividades_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriasServicio_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ColaSincronizacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    TablaAfectada = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RegistroId = table.Column<int>(type: "INTEGER", nullable: false),
                    Operacion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DatosCambio = table.Column<string>(type: "TEXT", nullable: false),
                    Prioridad = table.Column<int>(type: "INTEGER", nullable: false),
                    Procesado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaProcesado = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaSincronizacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColaSincronizacion_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Clave = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "TEXT", nullable: false),
                    TipoValor = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ValorPorDefecto = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    EsEditable = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiereReinicio = table.Column<bool>(type: "INTEGER", nullable: false),
                    NivelAcceso = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "admin"),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesSistema_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    Tabla = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RegistroId = table.Column<int>(type: "INTEGER", nullable: true),
                    Accion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DatosAnteriores = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    DatosNuevos = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: true),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modulos_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "TEXT", nullable: false),
                    DatosAdicionales = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Leida = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaLeida = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Importante = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaExpiracion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CanalesEntrega = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreadaEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreadaPor = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecuenciasNumeracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Prefijo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    SiguienteNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuenciasNumeracion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecuenciasNumeracion_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Suscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembresiaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFinGracia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suscripciones_EstadosSuscripcion_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosSuscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suscripciones_Membresias_MembresiaId",
                        column: x => x.MembresiaId,
                        principalTable: "Membresias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Suscripciones_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Contrasena = table.Column<string>(type: "TEXT", nullable: false),
                    PrimerLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganizacionId = table.Column<int>(type: "INTEGER", nullable: true),
                    RolId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PrimerLoginEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoLogin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Organizaciones_OrganizacionId",
                        column: x => x.OrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroCaja = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Abierta = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OperadorUsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    SupervisorUsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashSync = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cajas_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CierresActividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracionHoras = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    TotalCajas = table.Column<int>(type: "INTEGER", nullable: false),
                    CajasConDiferencias = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalVentas = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TotalTransacciones = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalArticulosVendidos = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticulosAgotados = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticulosConStock = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalUnidadesRestantes = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorInventarioFinal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ValorMerma = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CerradaPor = table.Column<int>(type: "INTEGER", nullable: false),
                    SupervisadaPor = table.Column<int>(type: "INTEGER", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProblemasReportados = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresActividad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CierresActividad_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CombosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PrecioCombo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombosVenta_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActividadCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaServicioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActividadCategorias_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActividadCategorias_CategoriasServicio_CategoriaServicioId",
                        column: x => x.CategoriaServicioId,
                        principalTable: "CategoriasServicio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClavesActivacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoActivacion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SuscripcionesId = table.Column<int>(type: "INTEGER", nullable: false),
                    Generada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Utilizada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Expirada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Revocada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaUtilizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaRevocacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsosMaximos = table.Column<int>(type: "INTEGER", nullable: false),
                    UsosActuales = table.Column<int>(type: "INTEGER", nullable: false),
                    LoteGeneracion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    UtilizadaPorOrganizacionId = table.Column<int>(type: "INTEGER", nullable: true),
                    UtilizadaPorUsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    IpActivacion = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    GeneradaPor = table.Column<int>(type: "INTEGER", nullable: true),
                    RevocadaPor = table.Column<int>(type: "INTEGER", nullable: true),
                    RazonRevocacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClavesActivacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClavesActivacion_Organizaciones_UtilizadaPorOrganizacionId",
                        column: x => x.UtilizadaPorOrganizacionId,
                        principalTable: "Organizaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClavesActivacion_Suscripciones_SuscripcionesId",
                        column: x => x.SuscripcionesId,
                        principalTable: "Suscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CierresCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalTransacciones = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalItemsVendidos = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoVentas = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    EfectivoCalculado = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    TarjetasCalculado = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    SinpesCalculado = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    EfectivoDeclarado = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    DiferenciaEfectivo = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    CerradaPor = table.Column<int>(type: "INTEGER", nullable: false),
                    SupervisadaPor = table.Column<int>(type: "INTEGER", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProblemasReportados = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CierresCaja_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransaccionesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CajaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroTransaccion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NumeroFactura = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    EstadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaTransaccion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    VendedorUsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashSync = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransaccionesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransaccionesVenta_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransaccionesVenta_EstadosVenta_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductosCategorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActividadCategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    CantidadInicial = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadActual = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadAlerta = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActualizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualizadoEn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HashSync = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosCategorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosCategorias_ActividadCategorias_ActividadCategoriaId",
                        column: x => x.ActividadCategoriaId,
                        principalTable: "ActividadCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosTransacciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransaccionId = table.Column<int>(type: "INTEGER", nullable: false),
                    MetodoPagoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProcesadoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcesadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosTransacciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosTransacciones_MetodosPago_MetodoPagoId",
                        column: x => x.MetodoPagoId,
                        principalTable: "MetodosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosTransacciones_TransaccionesVenta_TransaccionId",
                        column: x => x.TransaccionId,
                        principalTable: "TransaccionesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComboArticulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComboId = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticuloId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboArticulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComboArticulos_CombosVenta_ComboId",
                        column: x => x.ComboId,
                        principalTable: "CombosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboArticulos_ProductosCategorias_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "ProductosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetallesTransaccionesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransaccionId = table.Column<int>(type: "INTEGER", nullable: false),
                    ArticuloId = table.Column<int>(type: "INTEGER", nullable: true),
                    ComboId = table.Column<int>(type: "INTEGER", nullable: true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    EsCombo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesTransaccionesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesTransaccionesVenta_CombosVenta_ComboId",
                        column: x => x.ComboId,
                        principalTable: "CombosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DetallesTransaccionesVenta_ProductosCategorias_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "ProductosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DetallesTransaccionesVenta_TransaccionesVenta_TransaccionId",
                        column: x => x.TransaccionId,
                        principalTable: "TransaccionesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosInventario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArticuloId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoMovimientoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadAnterior = table.Column<int>(type: "INTEGER", nullable: false),
                    CantidadPosterior = table.Column<int>(type: "INTEGER", nullable: false),
                    CostoUnitario = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    TransaccionVentaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Justificacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RealizadoPor = table.Column<int>(type: "INTEGER", nullable: false),
                    AutorizadoPor = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sincronizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUltimaSync = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_ProductosCategorias_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "ProductosCategorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_TiposMovimientoInventario_TipoMovimientoId",
                        column: x => x.TipoMovimientoId,
                        principalTable: "TiposMovimientoInventario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventario_TransaccionesVenta_TransaccionVentaId",
                        column: x => x.TransaccionVentaId,
                        principalTable: "TransaccionesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "EstadosActividad",
                columns: new[] { "Id", "Activo", "CreadoEn", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4907), "Actividad no iniciada", "Sin Iniciar" },
                    { 2, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4912), "Actividad en desarrollo", "En Curso" },
                    { 3, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4915), "Actividad completada", "Terminada" },
                    { 4, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4920), "Actividad cancelada", "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "EstadosSuscripcion",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre", "PermiteUsoSistema" },
                values: new object[,]
                {
                    { 1, true, "Suscripción activa", "Activa", true },
                    { 2, true, "Suscripción suspendida", "Suspendida", false },
                    { 3, true, "Suscripción cancelada", "Cancelada", false }
                });

            migrationBuilder.InsertData(
                table: "EstadosVenta",
                columns: new[] { "Id", "Activo", "CreadoEn", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5010), "Venta pendiente de procesamiento", "Pendiente" },
                    { 2, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5015), "Venta completada exitosamente", "Completada" },
                    { 3, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5018), "Venta cancelada", "Cancelada" }
                });

            migrationBuilder.InsertData(
                table: "MetodosPago",
                columns: new[] { "Id", "Activo", "CreadoEn", "Descripcion", "Nombre", "RequiereReferencia" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5095), "Pago en efectivo", "Efectivo", false },
                    { 2, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5100), "Pago con tarjeta de crédito/débito", "Tarjeta", true },
                    { 3, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5104), "Pago con SINPE Móvil", "SINPE Móvil", true }
                });

            migrationBuilder.InsertData(
                table: "Organizaciones",
                columns: new[] { "Id", "Activo", "CorreoContacto", "CreadoEn", "Direccion", "Nombre", "PersonaAdquiriente", "TelefonoContacto" },
                values: new object[] { 1, true, "demo@gesco.com", new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4162), "San José, Costa Rica", "Organización Demo", "Administrador Demo", "2222-2222" });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Activo", "CreadoEn", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4670), "Acceso completo al sistema", "Administrador" },
                    { 2, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4685), "Acceso a ventas y caja", "Vendedor" },
                    { 3, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4689), "Supervisión de actividades", "Supervisor" }
                });

            migrationBuilder.InsertData(
                table: "TiposMovimientoInventario",
                columns: new[] { "Id", "Activo", "AfectaStock", "CreadoEn", "Descripcion", "Nombre", "RequiereJustificacion" },
                values: new object[,]
                {
                    { 1, true, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5187), "Entrada de mercancía al inventario", "Entrada", false },
                    { 2, true, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5192), "Salida por venta de productos", "Venta", false },
                    { 3, true, true, new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(5196), "Ajuste de inventario por diferencias", "Ajuste", true }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Activo", "ActualizadoEn", "ActualizadoPor", "Contrasena", "Correo", "CreadoEn", "CreadoPor", "NombreCompleto", "NombreUsuario", "OrganizacionId", "PrimerLogin", "PrimerLoginEn", "RolId", "Telefono", "UltimoLogin" },
                values: new object[] { 1, true, null, null, "$2a$11$rBNh2aFXK3H8JQhY0z5NXOmL7sPQCHfXOQrpPz0YNhzQHquPHH0Hy", "admin@gesco.com", new DateTime(2025, 8, 28, 9, 43, 7, 974, DateTimeKind.Local).AddTicks(4811), null, "Administrador del Sistema", "admin", 1, true, null, 1, "8888-8888", null });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCategorias_ActividadId",
                table: "ActividadCategorias",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_ActividadCategorias_CategoriaServicioId",
                table: "ActividadCategorias",
                column: "CategoriaServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_EstadoId",
                table: "Actividades",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_OrganizacionId",
                table: "Actividades",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_ActividadId",
                table: "Cajas",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasServicio_OrganizacionId",
                table: "CategoriasServicio",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CierresActividad_ActividadId",
                table: "CierresActividad",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_CierresCaja_CajaId",
                table: "CierresCaja",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_ClavesActivacion_CodigoActivacion",
                table: "ClavesActivacion",
                column: "CodigoActivacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClavesActivacion_SuscripcionesId",
                table: "ClavesActivacion",
                column: "SuscripcionesId");

            migrationBuilder.CreateIndex(
                name: "IX_ClavesActivacion_UtilizadaPorOrganizacionId",
                table: "ClavesActivacion",
                column: "UtilizadaPorOrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaSincronizacion_OrganizacionId",
                table: "ColaSincronizacion",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboArticulos_ArticuloId",
                table: "ComboArticulos",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboArticulos_ComboId",
                table: "ComboArticulos",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_CombosVenta_ActividadId",
                table: "CombosVenta",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesSistema_OrganizacionId",
                table: "ConfiguracionesSistema",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesTransaccionesVenta_ArticuloId",
                table: "DetallesTransaccionesVenta",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesTransaccionesVenta_ComboId",
                table: "DetallesTransaccionesVenta",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesTransaccionesVenta_TransaccionId",
                table: "DetallesTransaccionesVenta",
                column: "TransaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_OrganizacionId",
                table: "LogsAuditoria",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_OrganizacionId",
                table: "Modulos",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_ArticuloId",
                table: "MovimientosInventario",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TipoMovimientoId",
                table: "MovimientosInventario",
                column: "TipoMovimientoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_TransaccionVentaId",
                table: "MovimientosInventario",
                column: "TransaccionVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_OrganizacionId",
                table: "Notificaciones",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosTransacciones_MetodoPagoId",
                table: "PagosTransacciones",
                column: "MetodoPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosTransacciones_TransaccionId",
                table: "PagosTransacciones",
                column: "TransaccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosCategorias_ActividadCategoriaId",
                table: "ProductosCategorias",
                column: "ActividadCategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_SecuenciasNumeracion_OrganizacionId",
                table: "SecuenciasNumeracion",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_EstadoId",
                table: "Suscripciones",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_MembresiaId",
                table: "Suscripciones",
                column: "MembresiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Suscripciones_OrganizacionId",
                table: "Suscripciones",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesVenta_CajaId",
                table: "TransaccionesVenta",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TransaccionesVenta_EstadoId",
                table: "TransaccionesVenta",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_OrganizacionId",
                table: "Usuarios",
                column: "OrganizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CierresActividad");

            migrationBuilder.DropTable(
                name: "CierresCaja");

            migrationBuilder.DropTable(
                name: "ClavesActivacion");

            migrationBuilder.DropTable(
                name: "ColaSincronizacion");

            migrationBuilder.DropTable(
                name: "ComboArticulos");

            migrationBuilder.DropTable(
                name: "ConfiguracionesSistema");

            migrationBuilder.DropTable(
                name: "DetallesTransaccionesVenta");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.DropTable(
                name: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "PagosTransacciones");

            migrationBuilder.DropTable(
                name: "SecuenciasNumeracion");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Suscripciones");

            migrationBuilder.DropTable(
                name: "CombosVenta");

            migrationBuilder.DropTable(
                name: "ProductosCategorias");

            migrationBuilder.DropTable(
                name: "TiposMovimientoInventario");

            migrationBuilder.DropTable(
                name: "MetodosPago");

            migrationBuilder.DropTable(
                name: "TransaccionesVenta");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "EstadosSuscripcion");

            migrationBuilder.DropTable(
                name: "Membresias");

            migrationBuilder.DropTable(
                name: "ActividadCategorias");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "EstadosVenta");

            migrationBuilder.DropTable(
                name: "CategoriasServicio");

            migrationBuilder.DropTable(
                name: "Actividades");

            migrationBuilder.DropTable(
                name: "EstadosActividad");

            migrationBuilder.DropTable(
                name: "Organizaciones");
        }
    }
}

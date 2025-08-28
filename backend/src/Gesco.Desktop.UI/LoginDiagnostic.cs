using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Gesco.Desktop.Data.Context;
using Gesco.Desktop.Data.Entities;
using BCrypt.Net;

namespace Gesco.Desktop.UI.Utils
{
    /// <summary>
    /// Script de diagnóstico para problemas de login
    /// Ejecutar desde Program.cs en modo desarrollo para diagnosticar problemas de autenticación
    /// </summary>
    public static class LoginDiagnostic
    {
        public static async Task RunDiagnosticAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LocalDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("🔍 DIAGNÓSTICO DE LOGIN - GESCO DESKTOP");
            Console.WriteLine(new string('=', 60));

            try
            {
                // 1. Verificar conexión a base de datos
                Console.WriteLine("\n1️⃣ Verificando conexión a base de datos...");
                var canConnect = await context.Database.CanConnectAsync();
                Console.WriteLine($"   Conexión DB: {(canConnect ? "✅ OK" : "❌ FALLO")}");

                if (!canConnect)
                {
                    Console.WriteLine("   ❌ No se puede conectar a la base de datos. Verifica la configuración.");
                    return;
                }

                // 2. Verificar si existen usuarios
                Console.WriteLine("\n2️⃣ Verificando usuarios en base de datos...");
                var userCount = await context.Usuarios.CountAsync();
                Console.WriteLine($"   Total usuarios: {userCount}");

                if (userCount == 0)
                {
                    Console.WriteLine("   ⚠️ No hay usuarios en la base de datos");
                    await CreateDefaultUserAsync(context);
                    return;
                }

                // 3. Verificar usuario admin específico
                Console.WriteLine("\n3️⃣ Verificando usuario 'admin'...");
                var adminUser = await context.Usuarios
                    .Include(u => u.Organizacion)
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.NombreUsuario == "admin");

                if (adminUser == null)
                {
                    Console.WriteLine("   ❌ Usuario 'admin' no encontrado");
                    await CreateDefaultUserAsync(context);
                    return;
                }

                Console.WriteLine($"   ✅ Usuario encontrado:");
                Console.WriteLine($"      ID: {adminUser.Id}");
                Console.WriteLine($"      Usuario: {adminUser.NombreUsuario}");
                Console.WriteLine($"      Email: {adminUser.Correo}");
                Console.WriteLine($"      Activo: {adminUser.Activo}");
                Console.WriteLine($"      Organización: {adminUser.OrganizacionId}");
                Console.WriteLine($"      Rol: {adminUser.RolId} ({adminUser.Rol?.Nombre ?? "Sin rol"})");

                // 4. Verificar hash de contraseña
                Console.WriteLine("\n4️⃣ Verificando hash de contraseña...");
                var storedHash = adminUser.Contrasena;
                Console.WriteLine($"   Hash almacenado: {storedHash.Substring(0, 20)}...");

                // Probar verificación con diferentes contraseñas comunes
                var testPasswords = new[] { "admin123", "admin", "123456", "password" };
                bool passwordFound = false;

                foreach (var testPassword in testPasswords)
                {
                    try
                    {
                        bool isValid = BCrypt.Net.BCrypt.Verify(testPassword, storedHash);
                        Console.WriteLine($"   Probando '{testPassword}': {(isValid ? "✅ VÁLIDA" : "❌ inválida")}");
                        
                        if (isValid)
                        {
                            passwordFound = true;
                            Console.WriteLine($"   🎉 La contraseña correcta es: '{testPassword}'");
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ❌ Error verificando '{testPassword}': {ex.Message}");
                    }
                }

                if (!passwordFound)
                {
                    Console.WriteLine("   ⚠️ Ninguna contraseña común funcionó. Regenerando hash...");
                    await RegenerateAdminPasswordAsync(context, adminUser);
                }

                // 5. Verificar roles y organización
                Console.WriteLine("\n5️⃣ Verificando datos relacionados...");
                
                var orgCount = await context.Organizaciones.CountAsync();
                var roleCount = await context.Roles.CountAsync();
                
                Console.WriteLine($"   Organizaciones: {orgCount}");
                Console.WriteLine($"   Roles: {roleCount}");

                if (orgCount == 0 || roleCount == 0)
                {
                    Console.WriteLine("   ⚠️ Faltan datos semilla. Regenerando...");
                    await EnsureSeedDataAsync(context);
                }

                // 6. Prueba de autenticación completa
                Console.WriteLine("\n6️⃣ Realizando prueba de autenticación completa...");
                await TestAuthenticationFlowAsync(context);

                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("✅ DIAGNÓSTICO COMPLETADO");
                Console.WriteLine("🔑 Credenciales de prueba:");
                Console.WriteLine("   Usuario: admin");
                Console.WriteLine("   Contraseña: admin123");
                Console.WriteLine(new string('=', 60) + "\n");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante diagnóstico de login");
                Console.WriteLine($"❌ Error durante diagnóstico: {ex.Message}");
            }
        }

        private static async Task CreateDefaultUserAsync(LocalDbContext context)
        {
            Console.WriteLine("\n🔧 Creando usuario por defecto...");

            // Verificar/crear organización
            var org = await context.Organizaciones.FirstOrDefaultAsync();
            if (org == null)
            {
                org = new Organizacion
                {
                    Id = 1,
                    Nombre = "Organización Demo",
                    CorreoContacto = "demo@gesco.com",
                    TelefonoContacto = "2222-2222",
                    Direccion = "San José, Costa Rica",
                    PersonaAdquiriente = "Administrador Demo",
                    Activo = true,
                    CreadoEn = DateTime.Now
                };
                context.Organizaciones.Add(org);
                await context.SaveChangesAsync();
                Console.WriteLine("   ✅ Organización creada");
            }

            // Verificar/crear rol
            var rol = await context.Roles.FirstOrDefaultAsync(r => r.Id == 1);
            if (rol == null)
            {
                rol = new Rol
                {
                    Id = 1,
                    Nombre = "Administrador",
                    Descripcion = "Acceso completo al sistema",
                    Activo = true,
                    CreadoEn = DateTime.Now
                };
                context.Roles.Add(rol);
                await context.SaveChangesAsync();
                Console.WriteLine("   ✅ Rol creado");
            }

            // Crear usuario admin
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123", BCrypt.Net.BCrypt.GenerateSalt(12));
            var adminUser = new Usuario
            {
                NombreUsuario = "admin",
                Correo = "admin@gesco.com",
                NombreCompleto = "Administrador del Sistema",
                Telefono = "8888-8888",
                Contrasena = hashedPassword,
                OrganizacionId = org.Id,
                RolId = rol.Id,
                Activo = true,
                PrimerLogin = true,
                CreadoEn = DateTime.Now
            };

            context.Usuarios.Add(adminUser);
            await context.SaveChangesAsync();

            Console.WriteLine("   ✅ Usuario admin creado exitosamente");
            Console.WriteLine($"   🔑 Hash generado: {hashedPassword.Substring(0, 20)}...");
        }

        private static async Task RegenerateAdminPasswordAsync(LocalDbContext context, Usuario adminUser)
        {
            var newPassword = "admin123";
            var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt(12));

            adminUser.Contrasena = newHash;
            adminUser.ActualizadoEn = DateTime.Now;

            await context.SaveChangesAsync();

            Console.WriteLine($"   ✅ Hash regenerado para contraseña: {newPassword}");
            Console.WriteLine($"   🔑 Nuevo hash: {newHash.Substring(0, 20)}...");
        }

        private static async Task EnsureSeedDataAsync(LocalDbContext context)
        {
            // Crear datos semilla básicos si no existen
            if (!await context.Organizaciones.AnyAsync())
            {
                context.Organizaciones.Add(new Organizacion
                {
                    Nombre = "Organización Demo",
                    CorreoContacto = "demo@gesco.com",
                    Activo = true,
                    CreadoEn = DateTime.Now
                });
            }

            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Rol { Nombre = "Administrador", Descripcion = "Acceso completo", Activo = true, CreadoEn = DateTime.Now },
                    new Rol { Nombre = "Vendedor", Descripcion = "Acceso a ventas", Activo = true, CreadoEn = DateTime.Now },
                    new Rol { Nombre = "Supervisor", Descripcion = "Supervisión", Activo = true, CreadoEn = DateTime.Now }
                );
            }

            await context.SaveChangesAsync();
            Console.WriteLine("   ✅ Datos semilla completados");
        }

        private static async Task TestAuthenticationFlowAsync(LocalDbContext context)
        {
            var user = await context.Usuarios
                .Include(u => u.Organizacion)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == "admin");

            if (user == null)
            {
                Console.WriteLine("   ❌ No se pudo encontrar usuario para prueba");
                return;
            }

            // Simular el flujo completo de autenticación
            Console.WriteLine("   📋 Simulando flujo de autenticación...");

            // 1. Verificar usuario existe y está activo
            if (!user.Activo)
            {
                Console.WriteLine("   ❌ Usuario inactivo");
                return;
            }

            // 2. Verificar contraseña
            bool passwordValid = BCrypt.Net.BCrypt.Verify("admin123", user.Contrasena);
            Console.WriteLine($"   Verificación contraseña: {(passwordValid ? "✅ OK" : "❌ FALLO")}");

            // 3. Verificar datos relacionados
            bool hasOrganization = user.OrganizacionId.HasValue && user.Organizacion != null;
            bool hasRole = user.Rol != null;

            Console.WriteLine($"   Organización: {(hasOrganization ? "✅ OK" : "❌ FALLO")}");
            Console.WriteLine($"   Rol: {(hasRole ? "✅ OK" : "❌ FALLO")}");

            if (passwordValid && hasOrganization && hasRole)
            {
                Console.WriteLine("   🎉 Autenticación simulada: EXITOSA");
            }
            else
            {
                Console.WriteLine("   ❌ Autenticación simulada: FALLO");
            }
        }

        /// <summary>
        /// Método helper para generar hash de cualquier contraseña
        /// </summary>
        public static void GeneratePasswordHash(string password)
        {
            Console.WriteLine($"\n🔐 Generando hash para contraseña: '{password}'");
            
            var hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            var verification = BCrypt.Net.BCrypt.Verify(password, hash);
            
            Console.WriteLine($"   Hash generado: {hash}");
            Console.WriteLine($"   Verificación: {(verification ? "✅ OK" : "❌ FALLO")}");
            Console.WriteLine($"   Para usar en código C#:");
            Console.WriteLine($"   Contrasena = \"{hash}\"");
        }
    }

    // Método de extensión para usar en Program.cs
    public static class ServiceCollectionExtensions
    {
        public static async Task RunLoginDiagnosticAsync(this IServiceProvider services)
        {
            await LoginDiagnostic.RunDiagnosticAsync(services);
        }
    }
}
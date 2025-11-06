using Microsoft.AspNetCore.Mvc;
using ReservaApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaApp.Controllers
{
    public class TestController : Controller
    {
        private readonly SistemaReservasContext _context;

        public TestController(SistemaReservasContext context)
        {
            _context = context;
        }

        // GET: Test/Session
        public IActionResult Session()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userRole = HttpContext.Session.GetString("UserRole");

            ViewBag.UserId = userId;
            ViewBag.UserName = userName;
            ViewBag.UserRole = userRole;
            ViewBag.IsSessionAvailable = HttpContext.Session.IsAvailable;

            return View();
        }

        // GET: Test/CreateTestUser
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                // Verificar si ya existe el usuario de prueba
                var existingUser = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == "test@test.com");

                if (existingUser != null)
                {
                    ViewBag.Message = "El usuario de prueba ya existe. Email: test@test.com, Password: test123";
                    ViewBag.UserId = existingUser.IdUsuario;
                    return View();
                }

                // Crear usuario de prueba
                var testUser = new Models.Usuario
                {
                    Nombre = "Usuario de Prueba",
                    Email = "test@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                    Rol = "usuario",
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.Usuarios.Add(testUser);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Usuario de prueba creado exitosamente!";
                ViewBag.Email = "test@test.com";
                ViewBag.Password = "test123";
                ViewBag.UserId = testUser.IdUsuario;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al crear usuario: {ex.Message}";
                return View();
            }
        }

        // GET: Test/LoginTestUser
        public IActionResult LoginTestUser()
        {
            try
            {
                var testUser = _context.Usuarios
                    .FirstOrDefault(u => u.Email == "test@test.com" && u.Activo);

                if (testUser == null)
                {
                    ViewBag.Message = "Usuario de prueba no encontrado. Por favor, créalo primero.";
                    return View();
                }

                // Establecer sesión
                HttpContext.Session.SetInt32("UserId", testUser.IdUsuario);
                HttpContext.Session.SetString("UserName", testUser.Nombre);
                HttpContext.Session.SetString("UserRole", testUser.Rol);

                ViewBag.Message = "Sesión iniciada exitosamente!";
                ViewBag.UserId = testUser.IdUsuario;
                ViewBag.UserName = testUser.Nombre;
                ViewBag.UserRole = testUser.Rol;

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al iniciar sesión: {ex.Message}";
                return View();
            }
        }

        // GET: Test/CheckDatabase
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var usuarios = await _context.Usuarios.ToListAsync();
                var eventos = await _context.Eventos.ToListAsync();

                ViewBag.TotalUsuarios = usuarios.Count;
                ViewBag.TotalEventos = eventos.Count;
                ViewBag.Usuarios = usuarios;
                ViewBag.Message = "Conexión a la base de datos exitosa!";

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error al conectar con la base de datos: {ex.Message}";
                ViewBag.InnerException = ex.InnerException?.Message;
                return View();
            }
        }
    }
}
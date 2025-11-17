using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models;
using ReservaApp.Models.ViewModels;
using BCrypt.Net;

namespace ReservaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SistemaReservasContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SistemaReservasContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Si ya está logueado, redirigir al home
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                _logger.LogInformation("Intento de login para: {Email}", model.Email);

                if (ModelState.IsValid)
                {
                    var usuario = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Email == model.Email && u.Activo == true);

                    if (usuario != null && BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
                    {
                        // Guardar información del usuario en sesión
                        HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                        HttpContext.Session.SetString("UserName", usuario.Nombre);
                        HttpContext.Session.SetString("UserRole", usuario.Rol);

                        _logger.LogInformation("Login exitoso para usuario: {UserId}", usuario.IdUsuario);

                        return RedirectToAction("Index", "Home");
                    }

                    _logger.LogWarning("Login fallido para: {Email}", model.Email);
                    ModelState.AddModelError("", "Email o contraseña incorrectos");
                }
                else
                {
                    _logger.LogWarning("ModelState inválido en login");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Error: {ErrorMessage}", error.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login");
                ModelState.AddModelError("", "Ocurrió un error al procesar su solicitud");
            }

            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            // Si ya está logueado, redirigir al home
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Intento de registro para: {Email}", model.Email);

                if (ModelState.IsValid)
                {
                    // Verificar si el email ya existe
                    var existingUser = await _context.Usuarios.AnyAsync(u => u.Email == model.Email);

                    if (existingUser)
                    {
                        _logger.LogWarning("Email ya registrado: {Email}", model.Email);
                        ModelState.AddModelError("Email", "Este email ya está registrado");
                        return View(model);
                    }

                    // Crear nuevo usuario
                    var usuario = new Usuario
                    {
                        Nombre = model.Nombre,
                        Email = model.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                        Rol = "usuario",
                        FechaCreacion = DateTime.Now,
                        Activo = true
                    };

                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Usuario registrado exitosamente: {UserId}", usuario.IdUsuario);

                    // Iniciar sesión automáticamente
                    HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                    HttpContext.Session.SetString("UserName", usuario.Nombre);
                    HttpContext.Session.SetString("UserRole", usuario.Rol);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("ModelState inválido en registro");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Error de validación: {ErrorMessage}", error.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Register para email: {Email}", model.Email);
                ModelState.AddModelError("", "Ocurrió un error al crear la cuenta. Por favor intente nuevamente.");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            _logger.LogInformation("Usuario cerrando sesión: {UserId}", HttpContext.Session.GetInt32("UserId"));
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models;
using ReservaApp.Models.ViewModels;
using BCrypt.Net;
using ReservaApp.Data;
using ReservaApp.Models.ViewModels;
using ReservaApp.Models;

namespace ReservaApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SistemaReservasContext _context;

        public AccountController(SistemaReservasContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
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

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Email o contraseña incorrectos");
            }

            return View(model);
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el email ya existe
                if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                {
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

                // Iniciar sesión automáticamente
                HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                HttpContext.Session.SetString("UserName", usuario.Nombre);
                HttpContext.Session.SetString("UserRole", usuario.Rol);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
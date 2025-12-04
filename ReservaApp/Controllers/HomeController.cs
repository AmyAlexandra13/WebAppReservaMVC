using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models;

namespace ReservaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SistemaReservasContext _context;

        public HomeController(ILogger<HomeController> logger, SistemaReservasContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            // Si es admin, cargar estadísticas
            if (userId != null && userRole == "admin")
            {
                try
                {
                    // Eventos creados por el admin
                    var eventosCreados = await _context.Eventos
                        .Where(e => e.IdOrganizador == userId.Value)
                        .CountAsync();

                    // Reservas totales de los eventos del admin
                    var reservasTotales = await _context.Reservas
                        .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value)
                        .CountAsync();

                    // Asistentes únicos (usuarios que han reservado)
                    var asistentesUnicos = await _context.Reservas
                        .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value && r.Estado == "confirmada")
                        .Select(r => r.IdUsuario)
                        .Distinct()
                        .CountAsync();

                    // Cupos reservados totales (suma de cantidad de cupos)
                    var cuposReservados = await _context.Reservas
                        .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value && r.Estado == "confirmada")
                        .SumAsync(r => (int?)r.CantidadCupos) ?? 0;

                    // Eventos activos
                    var eventosActivos = await _context.Eventos
                        .Where(e => e.IdOrganizador == userId.Value && e.Activo && e.Fecha > DateTime.Now)
                        .CountAsync();

                    // Próximo evento
                    var proximoEvento = await _context.Eventos
                        .Where(e => e.IdOrganizador == userId.Value && e.Activo && e.Fecha > DateTime.Now)
                        .OrderBy(e => e.Fecha)
                        .FirstOrDefaultAsync();

                    ViewBag.EventosCreados = eventosCreados;
                    ViewBag.ReservasTotales = reservasTotales;
                    ViewBag.AsistentesUnicos = asistentesUnicos;
                    ViewBag.CuposReservados = cuposReservados;
                    ViewBag.EventosActivos = eventosActivos;
                    ViewBag.ProximoEvento = proximoEvento;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al cargar estadísticas del admin");
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
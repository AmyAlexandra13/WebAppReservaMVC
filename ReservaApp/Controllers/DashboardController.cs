using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models.ViewModels;

namespace ReservaApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SistemaReservasContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(SistemaReservasContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index(
            string? evento,
            string? estado,
            string? lugar,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Solo admin puede acceder al dashboard
            if (userRole != "admin")
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder al dashboard.";
                return RedirectToAction("Index", "Home");
            }

            // Obtener eventos del admin para el filtro
            var eventosAdmin = await _context.Eventos
                .Where(e => e.IdOrganizador == userId.Value)
                .OrderByDescending(e => e.FechaCreacion)
                .Select(e => new { e.IdEvento, e.Titulo })
                .ToListAsync();

            ViewBag.Eventos = eventosAdmin;

            // Query base de reservas
            var query = _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(evento) && int.TryParse(evento, out int eventoId))
            {
                query = query.Where(r => r.IdEvento == eventoId);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            if (!string.IsNullOrEmpty(lugar))
            {
                query = query.Where(r => r.IdEventoNavigation.Lugar.Contains(lugar));
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(r => r.IdEventoNavigation.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(r => r.IdEventoNavigation.Fecha <= fechaFin.Value.AddDays(1).AddSeconds(-1));
            }

            // Obtener reservas filtradas
            var reservas = await query
                .OrderByDescending(r => r.FechaReserva)
                .Select(r => new DashboardReservaViewModel
                {
                    IdReserva = r.IdReserva,
                    NombreUsuario = r.IdUsuarioNavigation.Nombre,
                    EmailUsuario = r.IdUsuarioNavigation.Email,
                    TituloEvento = r.IdEventoNavigation.Titulo,
                    LugarEvento = r.IdEventoNavigation.Lugar,
                    FechaEvento = r.IdEventoNavigation.Fecha,
                    CantidadCupos = r.CantidadCupos,
                    Estado = r.Estado,
                    FechaReserva = r.FechaReserva,
                    CapacidadEvento = r.IdEventoNavigation.Capacidad,
                    CuposDisponibles = r.IdEventoNavigation.CuposDisponibles,
                    EventoActivo = r.IdEventoNavigation.Activo
                })
                .ToListAsync();

            // Estadísticas generales (sin filtros)
            var statsQuery = _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value);

            var totalEventos = await _context.Eventos
                .Where(e => e.IdOrganizador == userId.Value)
                .CountAsync();

            var totalReservas = await statsQuery.CountAsync();

            var reservasConfirmadas = await statsQuery
                .Where(r => r.Estado == "confirmada")
                .CountAsync();

            var asistentesUnicos = await statsQuery
                .Where(r => r.Estado == "confirmada")
                .Select(r => r.IdUsuario)
                .Distinct()
                .CountAsync();

            var cuposReservados = await statsQuery
                .Where(r => r.Estado == "confirmada")
                .SumAsync(r => (int?)r.CantidadCupos) ?? 0;

            var ingresosTotales = await _context.Eventos
                .Where(e => e.IdOrganizador == userId.Value)
                .SumAsync(e => (int?)e.Capacidad) ?? 0;

            // Estadísticas para gráficos
            var reservasPorEstado = await statsQuery
                .GroupBy(r => r.Estado)
                .Select(g => new { Estado = g.Key, Count = g.Count() })
                .ToListAsync();

            var reservasPorEvento = await statsQuery
                .Where(r => r.Estado == "confirmada")
                .GroupBy(r => r.IdEventoNavigation.Titulo)
                .Select(g => new { Evento = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // Lugares más populares
            var lugaresPorReservas = await statsQuery
                .Where(r => r.Estado == "confirmada")
                .GroupBy(r => r.IdEventoNavigation.Lugar)
                .Select(g => new { Lugar = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // ViewBag para estadísticas
            ViewBag.TotalEventos = totalEventos;
            ViewBag.TotalReservas = totalReservas;
            ViewBag.ReservasConfirmadas = reservasConfirmadas;
            ViewBag.AsistentesUnicos = asistentesUnicos;
            ViewBag.CuposReservados = cuposReservados;
            ViewBag.CapacidadTotal = ingresosTotales;
            ViewBag.ReservasPorEstado = reservasPorEstado;
            ViewBag.ReservasPorEvento = reservasPorEvento;
            ViewBag.LugaresPorReservas = lugaresPorReservas;

            // Mantener filtros en la vista
            ViewBag.FiltroEvento = evento;
            ViewBag.FiltroEstado = estado;
            ViewBag.FiltroLugar = lugar;
            ViewBag.FiltroFechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FiltroFechaFin = fechaFin?.ToString("yyyy-MM-dd");

            return View(reservas);
        }

        // API: Exportar datos a CSV
        [HttpGet]
        public async Task<IActionResult> ExportarCSV(
            string? evento,
            string? estado,
            string? lugar,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || HttpContext.Session.GetString("UserRole") != "admin")
            {
                return Forbid();
            }

            // Query base
            var query = _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value);

            // Aplicar filtros
            if (!string.IsNullOrEmpty(evento) && int.TryParse(evento, out int eventoId))
            {
                query = query.Where(r => r.IdEvento == eventoId);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            if (!string.IsNullOrEmpty(lugar))
            {
                query = query.Where(r => r.IdEventoNavigation.Lugar.Contains(lugar));
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(r => r.IdEventoNavigation.Fecha >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(r => r.IdEventoNavigation.Fecha <= fechaFin.Value.AddDays(1).AddSeconds(-1));
            }

            var reservas = await query
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            // Generar CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Usuario,Email,Evento,Lugar,Fecha Evento,Cupos,Estado,Fecha Reserva");

            foreach (var r in reservas)
            {
                csv.AppendLine($"{r.IdReserva},{r.IdUsuarioNavigation.Nombre},{r.IdUsuarioNavigation.Email}," +
                    $"{r.IdEventoNavigation.Titulo},{r.IdEventoNavigation.Lugar}," +
                    $"{r.IdEventoNavigation.Fecha:yyyy-MM-dd HH:mm},{r.CantidadCupos},{r.Estado}," +
                    $"{r.FechaReserva:yyyy-MM-dd HH:mm}");
            }

            var fileName = $"reservas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }

        // API: Obtener datos para gráficos
        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null || HttpContext.Session.GetString("UserRole") != "admin")
            {
                return Forbid();
            }

            var query = _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.IdEventoNavigation.IdOrganizador == userId.Value);

            object data = null;

            switch (chartType)
            {
                case "porMes":
                    data = await query
                        .GroupBy(r => new { r.FechaReserva.Year, r.FechaReserva.Month })
                        .Select(g => new
                        {
                            mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                            total = g.Count()
                        })
                        .OrderBy(x => x.mes)
                        .Take(12)
                        .ToListAsync();
                    break;

                case "porLugar":
                    data = await query
                        .Where(r => r.Estado == "confirmada")
                        .GroupBy(r => r.IdEventoNavigation.Lugar)
                        .Select(g => new
                        {
                            lugar = g.Key,
                            total = g.Count()
                        })
                        .OrderByDescending(x => x.total)
                        .Take(10)
                        .ToListAsync();
                    break;

                case "porDia":
                    var ultimos7Dias = DateTime.Now.AddDays(-7);
                    data = await query
                        .Where(r => r.FechaReserva >= ultimos7Dias)
                        .GroupBy(r => r.FechaReserva.Date)
                        .Select(g => new
                        {
                            fecha = g.Key.ToString("yyyy-MM-dd"),
                            total = g.Count()
                        })
                        .OrderBy(x => x.fecha)
                        .ToListAsync();
                    break;
            }

            return Json(data);
        }
    }
}
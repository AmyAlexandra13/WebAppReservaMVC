using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models;
using ReservaApp.Models.ViewModels;

namespace ReservaApp.Controllers
{
    public class ReservasController : Controller
    {
        private readonly SistemaReservasContext _context;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(SistemaReservasContext context, ILogger<ReservasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reservas - Mis Reservas
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reservas = await _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Where(r => r.IdUsuario == userId.Value)
                .OrderByDescending(r => r.FechaReserva)
                .Select(r => new ReservaViewModel
                {
                    IdReserva = r.IdReserva,
                    TituloEvento = r.IdEventoNavigation.Titulo,
                    FechaEvento = r.IdEventoNavigation.Fecha,
                    LugarEvento = r.IdEventoNavigation.Lugar,
                    CantidadCupos = r.CantidadCupos,
                    Estado = r.Estado,
                    FechaReserva = r.FechaReserva,
                    FechaActualizacion = r.FechaActualizacion,
                    IdEvento = r.IdEvento,
                    EventoActivo = r.IdEventoNavigation.Activo,
                    CuposDisponibles = r.IdEventoNavigation.CuposDisponibles
                })
                .ToListAsync();

            return View(reservas);
        }

        // GET: Reservas/Eventos - Ver eventos disponibles
        public async Task<IActionResult> Eventos()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var eventos = await _context.Eventos
                .Include(e => e.IdOrganizadorNavigation)
                .Where(e => e.Activo && e.Fecha > DateTime.Now && e.CuposDisponibles > 0)
                .OrderBy(e => e.Fecha)
                .ToListAsync();

            // Obtener reservas del usuario para marcar eventos ya reservados
            var reservasUsuario = await _context.Reservas
                .Where(r => r.IdUsuario == userId.Value && r.Estado != "cancelada")
                .Select(r => new { r.IdEvento, r.CantidadCupos })
                .ToListAsync();

            var eventosViewModel = eventos.Select(e => new EventoDisponibleViewModel
            {
                IdEvento = e.IdEvento,
                Titulo = e.Titulo,
                Descripcion = e.Descripcion,
                Fecha = e.Fecha,
                Lugar = e.Lugar,
                Capacidad = e.Capacidad,
                CuposDisponibles = e.CuposDisponibles,
                NombreOrganizador = e.IdOrganizadorNavigation.Nombre,
                EmailOrganizador = e.IdOrganizadorNavigation.Email,
                TieneReserva = reservasUsuario.Any(r => r.IdEvento == e.IdEvento),
                CuposReservados = reservasUsuario.FirstOrDefault(r => r.IdEvento == e.IdEvento)?.CantidadCupos
            }).ToList();

            return View(eventosViewModel);
        }

        // GET: Reservas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var evento = await _context.Eventos
                .Include(e => e.IdOrganizadorNavigation)
                .FirstOrDefaultAsync(e => e.IdEvento == id);

            if (evento == null)
            {
                return NotFound();
            }

            // Verificar si el usuario ya tiene una reserva para este evento
            var reservaExistente = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdEvento == id && r.IdUsuario == userId.Value && r.Estado != "cancelada");

            var viewModel = new EventoDisponibleViewModel
            {
                IdEvento = evento.IdEvento,
                Titulo = evento.Titulo,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                Capacidad = evento.Capacidad,
                CuposDisponibles = evento.CuposDisponibles,
                NombreOrganizador = evento.IdOrganizadorNavigation.Nombre,
                EmailOrganizador = evento.IdOrganizadorNavigation.Email,
                TieneReserva = reservaExistente != null,
                CuposReservados = reservaExistente?.CantidadCupos
            };

            return View(viewModel);
        }

        // POST: Reservas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int eventoId, int cantidadCupos)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión" });
            }

            try
            {
                // Validar evento
                var evento = await _context.Eventos.FindAsync(eventoId);
                if (evento == null)
                {
                    return Json(new { success = false, message = "Evento no encontrado" });
                }

                if (!evento.Activo)
                {
                    return Json(new { success = false, message = "El evento no está activo" });
                }

                if (evento.Fecha <= DateTime.Now)
                {
                    return Json(new { success = false, message = "El evento ya ha pasado" });
                }

                if (evento.CuposDisponibles < cantidadCupos)
                {
                    return Json(new { success = false, message = "No hay suficientes cupos disponibles" });
                }

                // Verificar si ya tiene una reserva
                var reservaExistente = await _context.Reservas
                    .FirstOrDefaultAsync(r => r.IdEvento == eventoId && r.IdUsuario == userId.Value && r.Estado != "cancelada");

                if (reservaExistente != null)
                {
                    return Json(new { success = false, message = "Ya tienes una reserva para este evento" });
                }

                // Crear reserva
                var reserva = new Reserva
                {
                    IdUsuario = userId.Value,
                    IdEvento = eventoId,
                    CantidadCupos = cantidadCupos,
                    Estado = "confirmada",
                    FechaReserva = DateTime.Now,
                    FechaActualizacion = DateTime.Now
                };

                _context.Reservas.Add(reserva);

                // Actualizar cupos disponibles
                evento.CuposDisponibles -= cantidadCupos;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva creada - Usuario: {UserId}, Evento: {EventoId}, Cupos: {Cupos}",
                    userId.Value, eventoId, cantidadCupos);

                return Json(new
                {
                    success = true,
                    message = "Reserva creada exitosamente",
                    reservaId = reserva.IdReserva
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear reserva");
                return Json(new { success = false, message = "Error al crear la reserva" });
            }
        }

        // POST: Reservas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión" });
            }

            try
            {
                var reserva = await _context.Reservas
                    .Include(r => r.IdEventoNavigation)
                    .FirstOrDefaultAsync(r => r.IdReserva == id);

                if (reserva == null)
                {
                    return Json(new { success = false, message = "Reserva no encontrada" });
                }

                // Verificar que sea el dueño de la reserva
                if (reserva.IdUsuario != userId.Value)
                {
                    return Json(new { success = false, message = "No tiene permisos para cancelar esta reserva" });
                }

                if (reserva.Estado == "cancelada")
                {
                    return Json(new { success = false, message = "La reserva ya está cancelada" });
                }

                // Verificar que el evento no haya pasado
                if (reserva.IdEventoNavigation.Fecha <= DateTime.Now)
                {
                    return Json(new { success = false, message = "No se puede cancelar una reserva de un evento que ya pasó" });
                }

                // Cancelar reserva
                reserva.Estado = "cancelada";
                reserva.FechaActualizacion = DateTime.Now;

                // Devolver cupos al evento
                reserva.IdEventoNavigation.CuposDisponibles += reserva.CantidadCupos;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva cancelada - ID: {ReservaId}, Usuario: {UserId}", id, userId.Value);

                return Json(new { success = true, message = "Reserva cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar reserva");
                return Json(new { success = false, message = "Error al cancelar la reserva" });
            }
        }

        // GET: Reservas/DetallesReserva/5
        public async Task<IActionResult> DetallesReserva(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdEventoNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound();
            }

            // Verificar que sea el dueño de la reserva
            if (reserva.IdUsuario != userId.Value)
            {
                return Forbid();
            }

            var viewModel = new ReservaViewModel
            {
                IdReserva = reserva.IdReserva,
                TituloEvento = reserva.IdEventoNavigation.Titulo,
                FechaEvento = reserva.IdEventoNavigation.Fecha,
                LugarEvento = reserva.IdEventoNavigation.Lugar,
                CantidadCupos = reserva.CantidadCupos,
                Estado = reserva.Estado,
                FechaReserva = reserva.FechaReserva,
                FechaActualizacion = reserva.FechaActualizacion,
                IdEvento = reserva.IdEvento,
                EventoActivo = reserva.IdEventoNavigation.Activo,
                CuposDisponibles = reserva.IdEventoNavigation.CuposDisponibles,
                CapacidadTotal = reserva.IdEventoNavigation.Capacidad
            };

            return View(viewModel);
        }
    }
}
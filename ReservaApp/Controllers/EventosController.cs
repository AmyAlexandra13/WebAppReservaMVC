using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservaApp.Data;
using ReservaApp.Models;
using ReservaApp.Models.ViewModels;

namespace ReservaApp.Controllers
{
    public class EventosController : Controller
    {
        private readonly SistemaReservasContext _context;
        private readonly ILogger<EventosController> _logger;

        public EventosController(SistemaReservasContext context, ILogger<EventosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método auxiliar para verificar si el usuario es admin
        private bool IsAdmin()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole == "admin";
        }

        // GET: Eventos
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Solo admin puede acceder a la gestión de eventos
            if (userRole != "admin")
            {
                TempData["ErrorMessage"] = "No tienes permisos para acceder a esta sección.";
                return RedirectToAction("Eventos", "Reservas");
            }

            var eventos = await _context.Eventos
                .Include(e => e.IdOrganizadorNavigation)
                .OrderByDescending(e => e.FechaCreacion)
                .Select(e => new EventoViewModel
                {
                    IdEvento = e.IdEvento,
                    Titulo = e.Titulo,
                    Descripcion = e.Descripcion,
                    Fecha = e.Fecha,
                    Lugar = e.Lugar,
                    Capacidad = e.Capacidad,
                    CuposDisponibles = e.CuposDisponibles,
                    Activo = e.Activo,
                    NombreOrganizador = e.IdOrganizadorNavigation.Nombre,
                    EmailOrganizador = e.IdOrganizadorNavigation.Email,
                    FechaCreacion = e.FechaCreacion
                })
                .ToListAsync();

            return View(eventos);
        }

        // GET: Eventos/Details/5
        public async Task<IActionResult> Details(int? id)
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

            // Solo admin puede ver detalles desde esta vista
            if (!IsAdmin())
            {
                return RedirectToAction("Detalles", "Reservas", new { id });
            }

            var evento = await _context.Eventos
                .Include(e => e.IdOrganizadorNavigation)
                .Include(e => e.Reservas)
                .FirstOrDefaultAsync(m => m.IdEvento == id);

            if (evento == null)
            {
                return NotFound();
            }

            var viewModel = new EventoViewModel
            {
                IdEvento = evento.IdEvento,
                Titulo = evento.Titulo,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                Capacidad = evento.Capacidad,
                CuposDisponibles = evento.CuposDisponibles,
                Activo = evento.Activo,
                NombreOrganizador = evento.IdOrganizadorNavigation.Nombre,
                EmailOrganizador = evento.IdOrganizadorNavigation.Email,
                FechaCreacion = evento.FechaCreacion
            };

            return View(viewModel);
        }

        // GET: Eventos/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Solo admin puede crear eventos
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear eventos.";
                return RedirectToAction("Eventos", "Reservas");
            }

            var viewModel = new EventoViewModel
            {
                Fecha = DateTime.Now.AddDays(7),
                Activo = true
            };

            return View(viewModel);
        }

        // POST: Eventos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventoViewModel viewModel)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Solo admin puede crear eventos
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos para crear eventos" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var evento = new Evento
                    {
                        Titulo = viewModel.Titulo,
                        Descripcion = viewModel.Descripcion,
                        Fecha = viewModel.Fecha,
                        Lugar = viewModel.Lugar,
                        Capacidad = viewModel.Capacidad,
                        CuposDisponibles = viewModel.Capacidad,
                        IdOrganizador = userId.Value,
                        FechaCreacion = DateTime.Now,
                        Activo = viewModel.Activo
                    };

                    _context.Eventos.Add(evento);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Evento creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear evento");
                    ModelState.AddModelError("", "Error al crear el evento. Por favor intente nuevamente.");
                }
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            // Solo admin puede editar
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar eventos.";
                return RedirectToAction("Eventos", "Reservas");
            }

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return NotFound();
            }

            var viewModel = new EventoViewModel
            {
                IdEvento = evento.IdEvento,
                Titulo = evento.Titulo,
                Descripcion = evento.Descripcion,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                Capacidad = evento.Capacidad,
                CuposDisponibles = evento.CuposDisponibles,
                Activo = evento.Activo
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel viewModel)
        {
            if (id != viewModel.IdEvento)
            {
                return NotFound();
            }

            // Solo admin puede editar
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos para editar eventos" });
            }

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    evento.Titulo = viewModel.Titulo;
                    evento.Descripcion = viewModel.Descripcion;
                    evento.Fecha = viewModel.Fecha;
                    evento.Lugar = viewModel.Lugar;

                    int cuposReservados = evento.Capacidad - evento.CuposDisponibles;
                    evento.Capacidad = viewModel.Capacidad;
                    evento.CuposDisponibles = viewModel.Capacidad - cuposReservados;

                    evento.Activo = viewModel.Activo;

                    _context.Update(evento);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Evento actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventoExists(evento.IdEvento))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar evento");
                    ModelState.AddModelError("", "Error al actualizar el evento. Por favor intente nuevamente.");
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Solo admin puede eliminar
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos para eliminar eventos" });
            }

            var evento = await _context.Eventos
                .Include(e => e.Reservas)
                .FirstOrDefaultAsync(e => e.IdEvento == id);

            if (evento == null)
            {
                return Json(new { success = false, message = "Evento no encontrado" });
            }

            if (evento.Reservas.Any(r => r.Estado == "confirmada"))
            {
                return Json(new { success = false, message = "No se puede eliminar un evento con reservas confirmadas" });
            }

            try
            {
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Evento eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar evento");
                return Json(new { success = false, message = "Error al eliminar el evento" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            // Solo admin puede cambiar estado
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "No tienes permisos para modificar eventos" });
            }

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return Json(new { success = false, message = "Evento no encontrado" });
            }

            try
            {
                evento.Activo = !evento.Activo;
                await _context.SaveChangesAsync();

                return Json(new { success = true, activo = evento.Activo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del evento");
                return Json(new { success = false, message = "Error al cambiar el estado" });
            }
        }

        private bool EventoExists(int id)
        {
            return _context.Eventos.Any(e => e.IdEvento == id);
        }

        [HttpGet("api/eventos/{id}/reservas")]
        public async Task<IActionResult> GetReservas(int id)
        {
            // Solo admin puede ver las reservas
            if (!IsAdmin())
            {
                return Forbid();
            }

            try
            {
                var reservas = await _context.Reservas
                    .Include(r => r.IdUsuarioNavigation)
                    .Where(r => r.IdEvento == id)
                    .OrderByDescending(r => r.FechaReserva)
                    .Select(r => new
                    {
                        idReserva = r.IdReserva,
                        nombreUsuario = r.IdUsuarioNavigation.Nombre,
                        email = r.IdUsuarioNavigation.Email,
                        cantidadCupos = r.CantidadCupos,
                        estado = r.Estado,
                        fechaReserva = r.FechaReserva,
                        fechaActualizacion = r.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(reservas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reservas del evento {EventoId}", id);
                return StatusCode(500, new { message = "Error al cargar las reservas" });
            }
        }
    }
}
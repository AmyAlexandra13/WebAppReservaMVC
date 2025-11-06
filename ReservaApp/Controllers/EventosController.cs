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

        // GET: Eventos
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Evento> query = _context.Eventos.Include(e => e.IdOrganizadorNavigation);

            // Si no es admin, solo mostrar sus eventos
            if (userRole != "admin")
            {
                query = query.Where(e => e.IdOrganizador == userId.Value);
            }

            var eventos = await query
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

            var viewModel = new EventoViewModel
            {
                Fecha = DateTime.Now.AddDays(7), // Por defecto, una semana después
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
                        CuposDisponibles = viewModel.Capacidad, // Inicialmente todos disponibles
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

        // GET: Eventos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return NotFound();
            }

            // Verificar permisos: solo el organizador o admin puede editar
            if (userRole != "admin" && evento.IdOrganizador != userId)
            {
                return Forbid();
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

        // POST: Eventos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventoViewModel viewModel)
        {
            if (id != viewModel.IdEvento)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return NotFound();
            }

            // Verificar permisos
            if (userRole != "admin" && evento.IdOrganizador != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    evento.Titulo = viewModel.Titulo;
                    evento.Descripcion = viewModel.Descripcion;
                    evento.Fecha = viewModel.Fecha;
                    evento.Lugar = viewModel.Lugar;

                    // Ajustar cupos disponibles si cambia la capacidad
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

        // POST: Eventos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var evento = await _context.Eventos
                .Include(e => e.Reservas)
                .FirstOrDefaultAsync(e => e.IdEvento == id);

            if (evento == null)
            {
                return Json(new { success = false, message = "Evento no encontrado" });
            }

            // Verificar permisos
            if (userRole != "admin" && evento.IdOrganizador != userId)
            {
                return Json(new { success = false, message = "No tiene permisos para eliminar este evento" });
            }

            // Verificar si tiene reservas activas
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

        // POST: Eventos/ToggleActivo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null)
            {
                return Json(new { success = false, message = "Evento no encontrado" });
            }

            // Verificar permisos
            if (userRole != "admin" && evento.IdOrganizador != userId)
            {
                return Json(new { success = false, message = "No tiene permisos para modificar este evento" });
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
    }
}
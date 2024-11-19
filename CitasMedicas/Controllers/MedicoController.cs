using CitasMedicas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Controllers
{
    public class MedicoController : Controller
    {
        private readonly CitaMedica2Context _context;

        public MedicoController(CitaMedica2Context context)
        {
            _context = context;
        }


        public IActionResult PantallaMedico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = _context.Usuarios.Find(usuarioId);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario); // Pasa el modelo Usuario directamente a la vista
        }

        public IActionResult VerCitas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);

            if (usuarioId == null || medicoId == null)
            {
                return RedirectToAction("Login");
            }

            // Obtener todas las citas del médico logueado con Eager Loading
            var citas = _context.Citas
                .Include(c => c.IdPacienteNavigation) // Carga el Paciente
                    .ThenInclude(p => p.IdUsuarioNavigation) // Carga el Usuario asociado al Paciente
                .Where(c => c.IdMedico == medicoId.IdMedico) // Filtrar por ID del médico
                .ToList();

            return View(citas); // Pasa la lista de citas a la vista
        }

        public IActionResult PruebasDiagnosticas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login");
            }

            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);
            if (medicoId == null)
            {
                return NotFound();
            }

            // Obtener los IDs de los pacientes que han tenido citas con el médico
            var pacientesConCitas = _context.Citas
                .Where(c => c.IdMedico == medicoId.IdMedico)
                .Select(c => c.IdPaciente)
                .Distinct();

            // Obtener todas las pruebas diagnósticas de esos pacientes
            var pruebasDiagnosticas = _context.PruebasDiagnosticas
                .Include(p => p.IdPacienteNavigation)
                    .ThenInclude(p => p.IdUsuarioNavigation)
                .Where(p => pacientesConCitas.Contains(p.IdPaciente))
                .ToList();

            return View(pruebasDiagnosticas);
        }


        public IActionResult CrearPruebaDiagnostica()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);

            if (medicoId == null)
            {
                return RedirectToAction("Login");
            }

            // Obtener los pacientes a través de las citas
            var pacientes = _context.Citas
                .Include(c => c.IdPacienteNavigation) // Incluir el paciente
                    .ThenInclude(p => p.IdUsuarioNavigation) // Incluir el usuario del paciente
                .Where(c => c.IdMedico == medicoId.IdMedico)
                .Select(c => c.IdPacienteNavigation) // Seleccionar solo el paciente
                .Distinct()
                .ToList();

            ViewBag.Pacientes = pacientes;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GuardarPruebaDiagnostica(PruebasDiagnostica nuevaPrueba, IFormFile ImagenResultados)
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);

            if (medicoId == null)
            {
                return NotFound();
            }

            // Verificar si el paciente ha tenido citas con el médico
            var pacienteCitaValida = _context.Citas
                .Any(c => c.IdMedico == medicoId.IdMedico && c.IdPaciente == nuevaPrueba.IdPaciente);

            if (!pacienteCitaValida)
            {
                ModelState.AddModelError(string.Empty, "No se puede agregar la prueba diagnóstica. El paciente no ha tenido citas con este médico.");
                return View(nuevaPrueba); // Regresar a la vista con el modelo actual
            }

            if (ImagenResultados != null && ImagenResultados.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await ImagenResultados.CopyToAsync(memoryStream);
                    nuevaPrueba.Resultados = memoryStream.ToArray();
                }
            }

            _context.Add(nuevaPrueba);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PruebasDiagnosticas)); // Redireccionar a la lista de pruebas diagnósticas
        }




        public IActionResult HistorialMedico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login");
            }

            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);
            if (medicoId == null)
            {
                return NotFound();
            }

            // Obtener los IDs de los pacientes que han tenido citas con el médico
            var pacientesConCitas = _context.Citas
                .Where(c => c.IdMedico == medicoId.IdMedico)
                .Select(c => c.IdPaciente)
                .Distinct();

            // Obtener todos los historiales médicos de esos pacientes
            var historiales = _context.HistorialMedicos
                .Include(h => h.IdPacienteNavigation)
                    .ThenInclude(p => p.IdUsuarioNavigation)
                .Where(h => pacientesConCitas.Contains(h.IdPaciente))
                .ToList();

            return View(historiales);
        }


        public IActionResult GetHistorial(int pacienteId)
        {
            var historial = _context.HistorialMedicos
                .Include(h => h.IdPacienteNavigation) // Asegúrate de incluir el paciente
                    .ThenInclude(p => p.IdUsuarioNavigation) // Incluye el usuario del paciente
                .Where(h => h.IdPaciente == pacienteId)
                .ToList();

            return PartialView("_HistorialMedicoPartial", historial);
        }

        // GET: Medico/NuevoHistorial
        // GET: Medico/NuevoHistorial
        public IActionResult NuevoHistorial()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);

            if (medicoId == null)
            {
                return RedirectToAction("Login");
            }

            // Obtener solo los pacientes que han tenido citas con el médico logueado
            ViewBag.Pacientes = _context.Citas
                .Where(c => c.IdMedico == medicoId.IdMedico) // Filtrar citas por el ID del médico
                .Select(c => new {
                    IdPaciente = c.IdPacienteNavigation.IdPaciente,
                    NombreCompleto = $"{c.IdPacienteNavigation.IdUsuarioNavigation.Nombre} {c.IdPacienteNavigation.IdUsuarioNavigation.Apellido}"
                })
                .Distinct() // Eliminar duplicados
                .ToList(); // Crear la lista de pacientes

            return View();
        }


        // POST: Medico/NuevoHistorial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevoHistorial(HistorialMedico historialMedico)
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var medicoId = _context.Medicos.FirstOrDefault(m => m.IdUsuario == usuarioId.Value);

            if (medicoId == null)
            {
                return RedirectToAction("Login");
            }

            // Verificar si el paciente ha tenido citas con el médico
            var pacienteCitaValida = _context.Citas
                .Any(c => c.IdMedico == medicoId.IdMedico && c.IdPaciente == historialMedico.IdPaciente);

            if (!pacienteCitaValida)
            {
                ModelState.AddModelError(string.Empty, "No se puede agregar el historial médico. El paciente no ha tenido citas con este médico.");
                return View(historialMedico);
            }

            if (ModelState.IsValid)
            {
                _context.Add(historialMedico);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(HistorialMedico)); // Redirige a la lista de historiales médicos
            }

            // Si llegamos aquí, algo salió mal, vuelve a mostrar el formulario
            return View(historialMedico);
        }



        // GET: Medico/Recetas/5
        public IActionResult Recetas(int id)
        {
            // Obtén el historial médico por ID
            var historial = _context.HistorialMedicos
                .Include(h => h.Receta) // Asegúrate de incluir las recetas
                .ThenInclude(r => r.IdMedicamentoNavigation)
                .FirstOrDefault(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound();
            }

            return View(historial); // Pasa el historial a la vista
        }

      




        // GET: Medico/AgregarReceta/5
        public IActionResult AgregarReceta(int historialId)
        {
            // Puedes pasar el historialId a la vista para que el formulario sepa a qué historial pertenece la receta
            ViewBag.HistorialId = historialId;
            ViewBag.Medicamentos = _context.Medicamentos.ToList(); // Obtén la lista de medicamentos para el dropdown
            return View();
        }

        // POST: Medico/AgregarReceta
        [HttpPost]
        public IActionResult AgregarReceta(Receta receta)
        {
            if (ModelState.IsValid)
            {
                _context.Recetas.Add(receta);
                _context.SaveChanges();
                return RedirectToAction("Recetas", new { id = receta.IdHistorial });
            }
            return View(receta);
        }



    }
}

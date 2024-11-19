using CitasMedicas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly CitaMedica2Context _context;

        public AdministradorController(CitaMedica2Context context)
        {
            _context = context;
        }

        public IActionResult PaginaPrincipal()
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


        [HttpGet]
        public IActionResult RegistrarMedico()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegistrarMedico(Usuario usuario)
        {
            // Validar que el usuario sea mayor de 18 años
            var edadMinima = DateOnly.FromDateTime(DateTime.Now.AddYears(-18));
            if (usuario.FechaNacimiento > edadMinima)
            {
                ModelState.AddModelError("FechaNacimiento", "El usuario debe ser mayor de 18 años.");
                return View(usuario);
            }

            // Guardar la contraseña directamente (sin hasheo)
            //usuario.ContraseñaHash = usuario.ContraseñaHash; // Aquí deberías recoger el valor ingresado como contraseña

            // Asignar rol por defecto
            usuario.Rol = "Medico";

            _context.Usuarios.Add(usuario);
            _context.SaveChanges(); // Guarda el usuario y genera el IdUsuario automáticamente

            // Crear el médico
            var medico = new Medico
            {
                IdUsuario = usuario.IdUsuario // Asignar el IdUsuario del usuario creado
            };
            _context.Medicos.Add(medico);
            _context.SaveChanges();

            // Redirigir a la selección de especialidad
            return RedirectToAction("SeleccionarEspecialidad", new { idMedico = medico.IdMedico });
        }


        public IActionResult SeleccionarEspecialidad(int idMedico)
        {
            var especialidades = _context.Especialidades.ToList(); // Obtener todas las especialidades
            ViewBag.IdMedico = idMedico; // Pasar el Id del médico
            return View(especialidades);
        }

        [HttpPost]
        public IActionResult AsignarEspecialidad(int idMedico, int idEspecialidad)
        {
            var medico = _context.Medicos.Find(idMedico);
            if (medico != null)
            {
                medico.IdEspecialidad = idEspecialidad;
                _context.SaveChanges();
            }

            return RedirectToAction("PaginaPrincipal"); // Redirigir a la página principal o a donde desees
        }

        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(Usuario usuario, string password)
        {
            if (ModelState.IsValid)
            {
                // Asignar el rol de "Paciente" automáticamente
                usuario.Rol = "paciente"; // Asegúrate de tener una propiedad "Rol" en tu modelo Usuario
                usuario.ContraseñaHash = password; // Aquí se almacena la contraseña directamente sin hash.

                // Crear el nuevo usuario
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Una vez guardado el usuario, crear el paciente asociado
                var paciente = new Paciente
                {
                    IdUsuario = usuario.IdUsuario // Enlaza el paciente con el usuario recién creado
                };
                _context.Pacientes.Add(paciente);
                await _context.SaveChangesAsync();

                return RedirectToAction("PaginaPrincipal");
            }

            return View(usuario);
        }

        // Método GET para mostrar la vista de registro de medicamento
        [HttpGet]
        public IActionResult RegistrarMedicamento()
        {
            return View();
        }

        // Método POST para registrar un nuevo medicamento
        [HttpPost]
        public IActionResult RegistrarMedicamento(Medicamento medicamento)
        {
            if (ModelState.IsValid)
            {
                _context.Medicamentos.Add(medicamento);
                _context.SaveChanges();
                return RedirectToAction("PaginaPrincipal");
            }

            return View(medicamento);
        }

    }
}

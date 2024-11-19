using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CitasMedicas.Models;
using Microsoft.Build.Framework;

namespace CitasMedicas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly CitaMedica2Context _context;

        public UsuariosController(CitaMedica2Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Usuario usuario, string password)
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

                return RedirectToAction("Login");
            }

            return View(usuario);
        }



        // GET: Usuarios/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Email y contraseña son requeridos.");
                return View();
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || usuario.ContraseñaHash != password) // Comparación de contraseñas
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return View();
            }

            // Aquí puedes agregar lógica para redirigir según el rol
            if (usuario.Rol == "Medico")
            {
                HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                HttpContext.Session.SetString("UserEmail", usuario.Email);
                HttpContext.Session.SetString("UserRole", usuario.Rol);

                return RedirectToAction("PantallaMedico", "Medico"); // Redirigir a la vista de médicos
            }
            else if (usuario.Rol == "paciente")
            {
                HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                HttpContext.Session.SetString("UserEmail", usuario.Email);
                HttpContext.Session.SetString("UserRole", usuario.Rol);

                return RedirectToAction(nameof(Main)); // Redirigir a la vista de pacientes
            }

            else if (usuario.Rol == "admin")
            {
                HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
                HttpContext.Session.SetString("UserEmail", usuario.Email);
                HttpContext.Session.SetString("UserRole", usuario.Rol);

                return RedirectToAction("PaginaPrincipal", "Administrador"); // Redirigir a la vista de pacientes
            }

            return RedirectToAction(nameof(Index)); // O redirigir a la vista de usuarios
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Limpia la sesión
            return RedirectToAction("Login"); // Redirige a la página de inicio de sesión
        }



        // GET: Usuarios/Main
        public IActionResult Main()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var usuario = _context.Usuarios.Find(userId);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario); // Pasa el modelo Usuario directamente a la vista
        }

        


        public IActionResult VerCitas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");

            if (usuarioId == null)
            {
                return RedirectToAction("Login");
            }

            var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);

            if (paciente == null)
            {
                return NotFound();
            }

            // Usa la carga ansiosa para incluir los datos del médico
            var citas = _context.Citas
                .Include(c => c.IdMedicoNavigation) // Incluye el médico
                    .ThenInclude(m => m.IdUsuarioNavigation) // Incluye el usuario del médico
                .Where(c => c.IdPaciente == paciente.IdPaciente)
                .ToList();

            return View(citas);
        }


        public IActionResult AgendarNuevaCita()
        {
            // Obtener las especialidades
            ViewBag.Especialidades = _context.Especialidades.ToList(); // Asegúrate de tener la tabla de Especialidades

            return View();
        }

        [HttpPost]
        public IActionResult AgendarNuevaCita(Cita cita)
        {
            // Obtén el IdUsuario actual de la sesión
            var usuarioId = HttpContext.Session.GetInt32("UserId");

            if (ModelState.IsValid && usuarioId != null)
            {
                // Completar IdPaciente basado en el IdUsuario actual
                var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);
                if (paciente != null)
                {
                    cita.IdPaciente = paciente.IdPaciente; // Asignar el IdPaciente encontrado
                    _context.Citas.Add(cita);
                    _context.SaveChanges();
                    return RedirectToAction("VerCitas");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No se encontró un paciente asociado con el usuario actual.");
                }
            }

            ViewBag.Especialidades = _context.Especialidades.ToList();
            return View(cita);
        }


        [HttpGet]
        public IActionResult ObtenerMedicos(int especialidadId, DateTime fechaHora)
        {
            var medicos = _context.Medicos
            .Where(m => m.IdEspecialidad == especialidadId) // Filtra por especialidad
            .Select(m => new
            {
                idMedico = m.IdMedico,
                nombre = m.IdUsuarioNavigation.Nombre, // Asegúrate de que estas propiedades existan
                apellido = m.IdUsuarioNavigation.Apellido // Asegúrate de que estas propiedades existan
            })
            .ToList();

            return Json(medicos);
        }

        public IActionResult VerHistorialMedico()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);
            var historial = _context.HistorialMedicos.Where(h => h.IdPaciente == paciente.IdPaciente).ToList();
            return View(historial);
        }

        public IActionResult VerRecetasMedicas()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);
            var recetas = _context.Recetas
                .Include(r => r.IdHistorialNavigation) // Cargar historial asociado
                    .ThenInclude(h => h.IdPacienteNavigation) // Cargar paciente asociado al historial
                .Include(r => r.IdMedicamentoNavigation) // Cargar medicamento asociado
                .Where(r => r.IdHistorialNavigation.IdPaciente == paciente.IdPaciente)
                .ToList();
            return View(recetas);
        }



        // Método para mostrar alergias del paciente
        public IActionResult GestionarAlergias()
        {
            var usuarioId = HttpContext.Session.GetInt32("UserId");
            var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);
            if (paciente == null)
            {
                return NotFound("Paciente no encontrado");
            }

            var alergias = _context.Alergias.Where(a => a.IdPaciente == paciente.IdPaciente).ToList();
            return View(alergias);
        }

        // Método para mostrar el formulario de agregar alergia
        public IActionResult CrearAlergia()
        {
            return View();
        }

        // Método para agregar una nueva alergia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarAlergia(Alergia alergia)
        {
            if (ModelState.IsValid)
            {
                var usuarioId = HttpContext.Session.GetInt32("UserId");
                var paciente = _context.Pacientes.FirstOrDefault(p => p.IdUsuario == usuarioId.Value);

                if (paciente != null)
                {
                    alergia.IdPaciente = paciente.IdPaciente; // Asignar el IdPaciente
                    _context.Alergias.Add(alergia);
                    _context.SaveChanges();
                    return RedirectToAction("GestionarAlergias");
                }

                return NotFound("Paciente no encontrado");
            }
            return View("CrearAlergia", alergia);
        }



    }
}

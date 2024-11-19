using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class HistorialMedico
{
    public int IdHistorial { get; set; }

    public int? IdPaciente { get; set; }

    public DateOnly? Fecha { get; set; }

    public string? Descripcion { get; set; }

    public string? Diagnostico { get; set; }

    public string? Tratamiento { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();
}

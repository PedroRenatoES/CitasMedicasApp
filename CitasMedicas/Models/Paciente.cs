using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Paciente
{
    public int IdPaciente { get; set; }

    public int? IdUsuario { get; set; }

    public virtual ICollection<Alergia> Alergia { get; set; } = new List<Alergia>();

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual ICollection<HistorialMedico> HistorialMedicos { get; set; } = new List<HistorialMedico>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }

    public virtual ICollection<PruebasDiagnostica> PruebasDiagnosticas { get; set; } = new List<PruebasDiagnostica>();
}

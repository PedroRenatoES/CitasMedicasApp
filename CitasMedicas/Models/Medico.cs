using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Medico
{
    public int IdMedico { get; set; }

    public int? IdEspecialidad { get; set; }

    public int? IdUsuario { get; set; }

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public virtual Especialidade? IdEspecialidadNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}

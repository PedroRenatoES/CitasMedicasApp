using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Horario
{
    public int IdHorario { get; set; }

    public int? IdMedico { get; set; }

    public string? Dia { get; set; }

    public TimeOnly? HoraInicio { get; set; }

    public TimeOnly? HoraFin { get; set; }

    public virtual ICollection<HistorialHorario> HistorialHorarios { get; set; } = new List<HistorialHorario>();

    public virtual Medico? IdMedicoNavigation { get; set; }
}

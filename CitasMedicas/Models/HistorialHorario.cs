using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class HistorialHorario
{
    public int IdHistorial { get; set; }

    public int? IdHorario { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public string? Descripcion { get; set; }

    public virtual Horario? IdHorarioNavigation { get; set; }
}

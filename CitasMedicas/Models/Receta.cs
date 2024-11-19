using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Receta
{
    public int IdReceta { get; set; }

    public int? IdHistorial { get; set; }

    public int? IdMedicamento { get; set; }

    public string? Dosis { get; set; }

    public string? Duracion { get; set; }

    public DateOnly? FechaEmision { get; set; }

    public virtual HistorialMedico? IdHistorialNavigation { get; set; }

    public virtual Medicamento? IdMedicamentoNavigation { get; set; }
}

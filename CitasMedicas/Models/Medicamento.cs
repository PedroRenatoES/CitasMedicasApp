using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Medicamento
{
    public int IdMedicamento { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public string? Dosis { get; set; }

    public string? EfectosSecundarios { get; set; }

    public virtual ICollection<Receta> Receta { get; set; } = new List<Receta>();
}

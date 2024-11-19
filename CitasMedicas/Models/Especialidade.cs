using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Especialidade
{
    public int IdEspecialidad { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Medico> Medicos { get; set; } = new List<Medico>();
}

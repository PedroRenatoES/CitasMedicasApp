using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class Alergia
{
    public int IdAlergia { get; set; }

    public int? IdPaciente { get; set; }

    public string? Descripcion { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }
}

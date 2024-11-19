using System;
using System.Collections.Generic;

namespace CitasMedicas.Models;

public partial class PruebasDiagnostica
{
    public int IdPrueba { get; set; }

    public int? IdPaciente { get; set; }

    public string? TipoPrueba { get; set; }

    public DateOnly? Fecha { get; set; }

    public byte[]? Resultados { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }
}

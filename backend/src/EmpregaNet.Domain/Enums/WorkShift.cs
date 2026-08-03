using System.ComponentModel;

namespace EmpregaNet.Domain.Enums
{
    /// <summary>
    /// Turno ou escala de trabalho da vaga.
    /// </summary>
    public enum WorkShiftEnum
    {
        [Description("")] NaoSelecionado,
        [Description("Administrativo (comercial)")] Administrativo,
        [Description("1º turno")] PrimeiroTurno,
        [Description("2º turno")] SegundoTurno,
        [Description("3º turno (noturno)")] TerceiroTurno,
        [Description("Turno de revezamento")] Revezamento,
        [Description("Escala 12x36")] Escala12x36,
        [Description("Escala 6x1")] Escala6x1
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    /// <summary>
    /// Torna parcial a unicidade de <c>(JobId, UserId)</c> em <c>JobApplications</c>: passam a
    /// coexistir N candidaturas canceladas pelo candidato e, no máximo, uma activa.
    /// </summary>
    /// <remarks>
    /// Sem isto, permitir a recandidatura no código não bastava: a segunda candidatura à mesma vaga
    /// violava o índice único e rebentava no PostgreSQL. O provider InMemory dos testes não aplica
    /// índices, e por isso não acusava nada.
    ///
    /// <para>
    /// <b>Só toca no índice</b> — nenhum <c>rename</c> ou <c>drop</c> de coluna, nenhum dado
    /// reescrito. Recriar um índice é reversível na prática, e o <c>Down</c> repõe o índice total.
    /// Ainda assim, quem estiver a aplicar isto sobre uma base que já tenha duas candidaturas activas
    /// no mesmo par (impossível pelo índice anterior) veria a criação falhar — não é o caso aqui.
    /// </para>
    ///
    /// <para>
    /// O literal <c>9</c> no filtro é <c>ApplicationStatusEnum.CanceledByCandidate</c>. A coluna é
    /// <c>integer</c> e o enum não declara valores explícitos, portanto o SQL fica preso à posição do
    /// membro; quem impede esse número de passar a apontar para outro status é o teste
    /// <c>ApplicationStatusEnum_OrdemDosValores_DeveSerEstavel</c>.
    /// </para>
    /// </remarks>
    public partial class IndiceUnicoParcialDeCandidatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications",
                columns: new[] { "JobId", "UserId" },
                unique: true,
                filter: "\"Status\" <> 9");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobId_UserId",
                table: "JobApplications",
                columns: new[] { "JobId", "UserId" },
                unique: true);
        }
    }
}

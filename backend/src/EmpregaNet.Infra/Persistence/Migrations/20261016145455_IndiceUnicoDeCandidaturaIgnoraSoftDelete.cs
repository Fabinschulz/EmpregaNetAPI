using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    /// <summary>
    /// O índice único parcial de <c>(JobId, UserId)</c> em <c>JobApplications</c> passa a ignorar também
    /// as candidaturas apagadas por soft delete (<c>IsDeleted = true</c>).
    /// </summary>
    /// <remarks>
    /// O índice anterior só excluía <c>"Status" &lt;&gt; 9</c> (cancelada pelo candidato). Uma candidatura
    /// com <c>IsDeleted = true</c> continuava a ocupá-lo, mas o repositório
    /// (<c>JobApplicationRepository.IsActiveApplication</c>) já a trata como não activa: a verificação
    /// de existência dizia "não existe" e o <c>INSERT</c> rebentava com 23505 (HTTP 500).
    ///
    /// <para>
    /// <b>Só toca no índice</b> — nenhum <c>rename</c> ou <c>drop</c> de coluna, nenhum dado reescrito.
    /// O novo filtro é mais permissivo que o anterior, logo a criação não pode falhar por duplicados
    /// já existentes. O <c>Down</c> repõe o filtro anterior; só falharia se, entretanto, existirem duas
    /// candidaturas não canceladas no mesmo par, sendo uma delas soft-deleted.
    /// </para>
    /// </remarks>
    public partial class IndiceUnicoDeCandidaturaIgnoraSoftDelete : Migration
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
                filter: "\"Status\" <> 9 AND \"IsDeleted\" = false");
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
                unique: true,
                filter: "\"Status\" <> 9");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    /// <summary>
    /// Vaga passa a ter total de posições, contador de preenchidas e encerramento datado com motivo.
    /// </summary>
    /// <remarks>
    /// <b>Esta migration move dados.</b> Três backfills, todos sobre linhas já existentes:
    /// <list type="number">
    ///   <item>
    ///     <c>FilledPositions</c> recebe a contagem de candidaturas que ocupam posição - os estados
    ///     <c>Approved</c> (1) e <c>Finished</c> (8) de <c>ApplicationStatusEnum</c>, cujos inteiros
    ///     são contrato de dados. Sem isto, uma vaga que já aprovou gente nasceria com saldo cheio e
    ///     poderia contratar o dobro.
    ///   </item>
    ///   <item>
    ///     <c>Positions</c> sobe para a contagem quando ela passa de 1. O default de uma posição
    ///     descreve a vaga antiga típica, mas uma vaga que aprovou três pessoas tinha três posições -
    ///     deixá-la em 1 criaria de imediato <c>FilledPositions &gt; Positions</c>, invariante que o
    ///     agregado recusa.
    ///   </item>
    ///   <item>
    ///     Vagas já encerradas recebem <c>ClosureReason = 0</c> (Manual) e a data do último toque.
    ///     Antes desta capacidade o botão do recrutador era a única forma de encerrar, logo o motivo
    ///     é verdadeiro; e com ele preenchido, <c>ClosureReason NULL</c> passa a significar apenas
    ///     "vaga activa".
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// <b>A ordem importa.</b> As <c>CHECK</c> entram só depois dos backfills: entre o primeiro e o
    /// segundo, uma vaga antiga fica momentaneamente com mais preenchidas do que o total, e uma
    /// constraint já criada abortaria a própria migration.
    /// </para>
    ///
    /// <para>
    /// Uma vaga activa que já tenha preenchido tudo fica <b>activa e cheia</b>: a migration não
    /// encerra vagas em produção por uma regra que não existia quando foram publicadas. A candidatura
    /// é recusada com mensagem própria e o recrutador resolve aumentando o total na edição. É também
    /// a razão de não existir aqui uma constraint "activa implica posição livre".
    /// </para>
    ///
    /// <para>
    /// O <c>Down</c> apaga as quatro colunas e, com elas, todo o histórico de preenchimento e de
    /// motivo de encerramento - os dados não são recuperáveis por reaplicar o <c>Up</c>.
    /// </para>
    /// </remarks>
    public partial class VagasPorPosicaoEEncerramentoAutomatico : Migration
    {
        private const string PositionHoldingStatuses = "(1, 8)";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosedAt",
                table: "Jobs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosureReason",
                table: "Jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FilledPositions",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Positions",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql($"""
                UPDATE "Jobs" AS j
                SET "FilledPositions" = held."Count"
                FROM (
                    SELECT "JobId", COUNT(*)::int AS "Count"
                    FROM "JobApplications"
                    WHERE "IsDeleted" = FALSE
                      AND "Status" IN {PositionHoldingStatuses}
                    GROUP BY "JobId"
                ) AS held
                WHERE j."Id" = held."JobId";
                """);

            migrationBuilder.Sql("""
                UPDATE "Jobs"
                SET "Positions" = "FilledPositions"
                WHERE "FilledPositions" > "Positions";
                """);

            migrationBuilder.Sql("""
                UPDATE "Jobs"
                SET "ClosureReason" = 0,
                    "ClosedAt" = COALESCE("UpdatedAt", "CreatedAt")
                WHERE "IsActive" = FALSE
                  AND "ClosureReason" IS NULL;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jobs_FilledPositions",
                table: "Jobs",
                sql: "\"FilledPositions\" >= 0 AND \"FilledPositions\" <= \"Positions\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Jobs_Positions",
                table: "Jobs",
                sql: "\"Positions\" >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Jobs_FilledPositions",
                table: "Jobs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Jobs_Positions",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ClosureReason",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "FilledPositions",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Positions",
                table: "Jobs");
        }
    }
}

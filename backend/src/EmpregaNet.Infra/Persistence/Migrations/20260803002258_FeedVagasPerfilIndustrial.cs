using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    public partial class FeedVagasPerfilIndustrial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Technologies",
                table: "Jobs",
                newName: "Requirements");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_Technologies",
                table: "Jobs",
                newName: "IX_Jobs_Requirements");

            migrationBuilder.Sql("""UPDATE "Jobs" SET "Requirements" = '{}'::text[];""");

            migrationBuilder.RenameColumn(
                name: "Seniority",
                table: "Jobs",
                newName: "ExperienceLevel");

            migrationBuilder.AddColumn<int>(
                name: "WorkShift",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsPcdFriendly",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "Jobs" SET "Area" = CASE "Area"
                    WHEN 1  THEN 13  -- Development     -> Tecnologia da Informação
                    WHEN 2  THEN 16  -- Design          -> Outras
                    WHEN 3  THEN 10  -- Marketing       -> Comercial
                    WHEN 4  THEN 10  -- Sales           -> Comercial
                    WHEN 5  THEN 11  -- HumanResources  -> Recursos Humanos
                    WHEN 6  THEN 12  -- Finance         -> Financeiro
                    WHEN 7  THEN 1   -- Operations      -> Produção
                    WHEN 8  THEN 9   -- CustomerSupport -> Administrativo
                    WHEN 9  THEN 2   -- Logistics       -> Logística
                    WHEN 10 THEN 14  -- Health          -> Saúde
                    WHEN 11 THEN 15  -- Education       -> Educação
                    WHEN 12 THEN 16  -- Other           -> Outras
                    ELSE 0           -- NaoSelecionado permanece
                END;
                """);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Jobs",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('pt_unaccent', coalesce(\"Title\", '')), 'A') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Summary\", '')), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Requirements\")), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Benefits\")), 'C') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Description\", '')), 'D')",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "setweight(to_tsvector('pt_unaccent', coalesce(\"Title\", '')), 'A') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Summary\", '')), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Technologies\")), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Benefits\")), 'C') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Description\", '')), 'D')",
                oldStored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PcdFriendly",
                table: "Jobs",
                column: "IsPcdFriendly",
                filter: "\"IsPcdFriendly\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_PcdFriendly",
                table: "Jobs");

            migrationBuilder.Sql("""
                UPDATE "Jobs" SET "Area" = CASE "Area"
                    WHEN 1  THEN 7   -- Produção        -> Operations
                    WHEN 2  THEN 9   -- Logística       -> Logistics
                    WHEN 9  THEN 8   -- Administrativo  -> CustomerSupport
                    WHEN 10 THEN 4   -- Comercial       -> Sales
                    WHEN 11 THEN 5   -- Recursos Humanos-> HumanResources
                    WHEN 12 THEN 6   -- Financeiro      -> Finance
                    WHEN 13 THEN 1   -- TI              -> Development
                    WHEN 14 THEN 10  -- Saúde           -> Health
                    WHEN 15 THEN 11  -- Educação        -> Education
                    WHEN 0  THEN 0   -- NaoSelecionado permanece
                    ELSE 12          -- Sem equivalente -> Other
                END;
                """);

            migrationBuilder.DropColumn(
                name: "IsPcdFriendly",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "WorkShift",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "ExperienceLevel",
                table: "Jobs",
                newName: "Seniority");

            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "Jobs",
                newName: "Technologies");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_Requirements",
                table: "Jobs",
                newName: "IX_Jobs_Technologies");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Jobs",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('pt_unaccent', coalesce(\"Title\", '')), 'A') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Summary\", '')), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Technologies\")), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Benefits\")), 'C') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Description\", '')), 'D')",
                stored: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true,
                oldComputedColumnSql: "setweight(to_tsvector('pt_unaccent', coalesce(\"Title\", '')), 'A') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Summary\", '')), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Requirements\")), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Benefits\")), 'C') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Description\", '')), 'D')",
                oldStored: true);
        }
    }
}

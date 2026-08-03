using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    public partial class FeedVagasJobEnriquecido : Migration
    {
        private const int LegacyRemoteJobType = 8;
        private const int JobTypeFullTime = 1;
        private const int WorkModelRemote = 3;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_ts_config WHERE cfgname = 'pt_unaccent') THEN
                        CREATE TEXT SEARCH CONFIGURATION pt_unaccent ( COPY = portuguese );
                        ALTER TEXT SEARCH CONFIGURATION pt_unaccent
                            ALTER MAPPING FOR hword, hword_part, word
                            WITH unaccent, portuguese_stem;
                    END IF;
                END
                $$;
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION empreganet_array_to_text(text[])
                RETURNS text
                LANGUAGE sql
                IMMUTABLE
                PARALLEL SAFE
                AS $$ SELECT coalesce(array_to_string($1, ' '), '') $$;
                """);

            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "Jobs",
                newName: "SalaryMin");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryMin",
                table: "Jobs",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMax",
                table: "Jobs",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SalaryDisclosed",
                table: "Jobs",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkModel",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Seniority",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Jobs",
                type: "character varying(280)",
                maxLength: 280,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Technologies",
                table: "Jobs",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<List<string>>(
                name: "Benefits",
                table: "Jobs",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Jobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Jobs",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "BR");

            migrationBuilder.Sql($"""
                UPDATE "Jobs"
                   SET "WorkModel" = {WorkModelRemote},
                       "JobType"   = {JobTypeFullTime}
                 WHERE "JobType" = {LegacyRemoteJobType};
                """);

            migrationBuilder.Sql("""
                UPDATE "Jobs" j
                   SET "City"    = c."City",
                       "State"   = c."State",
                       "Country" = 'BR'
                  FROM "Companies" c
                 WHERE c."Id" = j."CompanyId";
                """);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Jobs",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "setweight(to_tsvector('pt_unaccent', coalesce(\"Title\", '')), 'A') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Summary\", '')), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Technologies\")), 'B') ||\nsetweight(to_tsvector('pt_unaccent', empreganet_array_to_text(\"Benefits\")), 'C') ||\nsetweight(to_tsvector('pt_unaccent', coalesce(\"Description\", '')), 'D')",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Feed",
                table: "Jobs",
                columns: new[] { "IsDeleted", "IsActive", "PublishedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Location",
                table: "Jobs",
                columns: new[] { "State", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SalaryMax",
                table: "Jobs",
                column: "SalaryMax",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SearchVector",
                table: "Jobs",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Technologies",
                table: "Jobs",
                column: "Technologies")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Benefits",
                table: "Jobs",
                column: "Benefits")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Name_Trgm",
                table: "Companies",
                column: "CompanyName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Companies_Name_Trgm", table: "Companies");
            migrationBuilder.DropIndex(name: "IX_Jobs_Benefits", table: "Jobs");
            migrationBuilder.DropIndex(name: "IX_Jobs_Technologies", table: "Jobs");
            migrationBuilder.DropIndex(name: "IX_Jobs_SearchVector", table: "Jobs");
            migrationBuilder.DropIndex(name: "IX_Jobs_SalaryMax", table: "Jobs");
            migrationBuilder.DropIndex(name: "IX_Jobs_Location", table: "Jobs");
            migrationBuilder.DropIndex(name: "IX_Jobs_Feed", table: "Jobs");

            migrationBuilder.DropColumn(name: "SearchVector", table: "Jobs");

            migrationBuilder.Sql($"""
                UPDATE "Jobs"
                   SET "JobType" = {LegacyRemoteJobType}
                 WHERE "WorkModel" = {WorkModelRemote};
                """);

            migrationBuilder.DropColumn(name: "Country", table: "Jobs");
            migrationBuilder.DropColumn(name: "State", table: "Jobs");
            migrationBuilder.DropColumn(name: "City", table: "Jobs");
            migrationBuilder.DropColumn(name: "Benefits", table: "Jobs");
            migrationBuilder.DropColumn(name: "Technologies", table: "Jobs");
            migrationBuilder.DropColumn(name: "Summary", table: "Jobs");
            migrationBuilder.DropColumn(name: "Area", table: "Jobs");
            migrationBuilder.DropColumn(name: "Seniority", table: "Jobs");
            migrationBuilder.DropColumn(name: "WorkModel", table: "Jobs");
            migrationBuilder.DropColumn(name: "SalaryDisclosed", table: "Jobs");
            migrationBuilder.DropColumn(name: "SalaryMax", table: "Jobs");

            migrationBuilder.Sql("""UPDATE "Jobs" SET "SalaryMin" = 0 WHERE "SalaryMin" IS NULL;""");

            migrationBuilder.AlterColumn<decimal>(
                name: "SalaryMin",
                table: "Jobs",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "SalaryMin",
                table: "Jobs",
                newName: "Salary");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS empreganet_array_to_text(text[]);");
        }
    }
}

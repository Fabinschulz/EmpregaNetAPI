using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Migrations
{
    /// <summary>
    /// Acrescenta o CPF do utilizador como identificador de login, a par do e-mail.
    /// </summary>
    public partial class CpfDoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Users",
                type: "character varying(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Cpf",
                table: "Users",
                column: "Cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Cpf",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Users");
        }
    }
}

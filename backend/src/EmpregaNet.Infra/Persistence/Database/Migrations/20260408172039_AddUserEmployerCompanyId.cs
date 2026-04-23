using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmpregaNet.Infra.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmployerCompanyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EmployerCompanyId",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployerCompanyId",
                table: "Users",
                column: "EmployerCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Companies_EmployerCompanyId",
                table: "Users",
                column: "EmployerCompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Companies_EmployerCompanyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmployerCompanyId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmployerCompanyId",
                table: "Users");
        }
    }
}

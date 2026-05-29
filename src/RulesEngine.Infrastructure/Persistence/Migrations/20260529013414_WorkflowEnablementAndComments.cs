using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowEnablementAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "workflows",
                type: "character varying(4000)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "workflows",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql(@"UPDATE ""workflows"" SET ""IsEnabled"" = ""IsActive"";");

            migrationBuilder.CreateIndex(
                name: "UX_workflows_Id_ActiveEnabled",
                table: "workflows",
                columns: new[] { "Id", "IsEnabled" },
                unique: true,
                filter: "\"IsActive\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_workflows_Id_ActiveEnabled",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "Comments",
                table: "workflows");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "workflows");
        }
    }
}

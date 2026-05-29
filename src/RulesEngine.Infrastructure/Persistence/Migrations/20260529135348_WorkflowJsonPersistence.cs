using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowJsonPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowJson",
                table: "workflows",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowJson",
                table: "workflows");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRuleExecuteOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecuteOrder",
                table: "rules",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecuteOrder",
                table: "rules");
        }
    }
}

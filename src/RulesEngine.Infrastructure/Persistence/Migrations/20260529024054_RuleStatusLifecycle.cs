using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RuleStatusLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "rules",
                type: "character varying(32)",
                nullable: false,
                defaultValue: "draft");

            migrationBuilder.AddCheckConstraint(
                name: "CK_rules_Status",
                table: "rules",
                sql: "\"Status\" IN ('draft', 'failed', 'disabled', 'production')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_rules_Status",
                table: "rules");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "rules");
        }
    }
}

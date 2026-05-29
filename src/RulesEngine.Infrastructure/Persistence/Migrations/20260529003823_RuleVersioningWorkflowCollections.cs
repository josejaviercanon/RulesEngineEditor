using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RuleVersioningWorkflowCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RuleGuidId",
                table: "rules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "workflow_rules",
                columns: table => new
                {
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersion = table.Column<int>(type: "integer", nullable: false),
                    RuleGuidId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_rules", x => new { x.WorkflowId, x.WorkflowVersion, x.RuleGuidId });
                    table.ForeignKey(
                        name: "FK_workflow_rules_workflows_WorkflowId_WorkflowVersion",
                        columns: x => new { x.WorkflowId, x.WorkflowVersion },
                        principalTable: "workflows",
                        principalColumns: new[] { "Id", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(@"
UPDATE ""rules""
SET ""RuleGuidId"" = ""Id""
WHERE ""RuleGuidId"" = '00000000-0000-0000-0000-000000000000'::uuid;
");

            migrationBuilder.Sql(@"
INSERT INTO ""rules"" (
    ""Id"", ""RuleGuidId"", ""Name"", ""Expression"", ""RuleJson"", ""Version"", ""IsActive"", ""EffectiveFromUtc"", ""EffectiveToUtc""
)
WITH parsed AS (
    SELECT
        (
            substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 1, 8)
            || '-'
            || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 9, 4)
            || '-'
            || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 13, 4)
            || '-'
            || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 17, 4)
            || '-'
            || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 21, 12)
        )::uuid AS ""DeterministicId"",
        coalesce(
            nullif(rule->>'RuleGuidId', '')::uuid,
            (
                substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 1, 8)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 9, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 13, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 17, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 21, 12)
            )::uuid
        ) AS ""RuleGuidId"",
        coalesce(rule->>'RuleName', '') AS ""RuleName"",
        coalesce(rule->>'Expression', '') AS ""RuleExpression"",
        rule::text AS ""RuleJson"",
        coalesce(nullif(rule->>'Version', '')::integer, 1) AS ""RuleVersion"",
        coalesce(nullif(rule->>'IsActive', '')::boolean, true) AS ""RuleIsActive""
    FROM ""workflows"" w
    CROSS JOIN LATERAL jsonb_array_elements(
        coalesce(((w.""Definition""->>'RuleJson')::jsonb->'Rules'), '[]'::jsonb)
    ) rule
)
SELECT
    p.""DeterministicId"",
    p.""RuleGuidId"",
    p.""RuleName"",
    p.""RuleExpression"",
    p.""RuleJson"",
    p.""RuleVersion"",
    p.""RuleIsActive"",
    NULL::timestamp with time zone,
    NULL::timestamp with time zone
FROM parsed p
WHERE NOT EXISTS (
    SELECT 1
    FROM ""rules"" r
    WHERE r.""RuleGuidId"" = p.""RuleGuidId""
      AND r.""Version"" = p.""RuleVersion""
);
");

            migrationBuilder.Sql(@"
INSERT INTO ""workflow_rules"" (""WorkflowId"", ""WorkflowVersion"", ""RuleGuidId"", ""RuleVersion"")
WITH parsed AS (
    SELECT
        w.""Id"" AS ""WorkflowId"",
        w.""Version"" AS ""WorkflowVersion"",
        coalesce(
            nullif(rule->>'RuleGuidId', '')::uuid,
            (
                substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 1, 8)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 9, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 13, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 17, 4)
                || '-'
                || substr(md5(w.""Id""::text || ':' || coalesce(rule->>'RuleName', '') || ':' || coalesce(rule->>'Expression', '')), 21, 12)
            )::uuid
        ) AS ""RuleGuidId"",
        coalesce(nullif(rule->>'Version', '')::integer, 1) AS ""RuleVersion""
    FROM ""workflows"" w
    CROSS JOIN LATERAL jsonb_array_elements(
        coalesce(((w.""Definition""->>'RuleJson')::jsonb->'Rules'), '[]'::jsonb)
    ) rule
)
SELECT DISTINCT
    p.""WorkflowId"",
    p.""WorkflowVersion"",
    p.""RuleGuidId"",
    p.""RuleVersion""
FROM parsed p
WHERE NOT EXISTS (
    SELECT 1
    FROM ""workflow_rules"" wr
    WHERE wr.""WorkflowId"" = p.""WorkflowId""
      AND wr.""WorkflowVersion"" = p.""WorkflowVersion""
      AND wr.""RuleGuidId"" = p.""RuleGuidId""
);
");

            migrationBuilder.CreateIndex(
                name: "IX_rules_RuleGuidId_IsActive_Version",
                table: "rules",
                columns: new[] { "RuleGuidId", "IsActive", "Version" });

            migrationBuilder.CreateIndex(
                name: "UX_rules_RuleGuidId_Active",
                table: "rules",
                column: "RuleGuidId",
                unique: true,
                filter: "\"IsActive\"");

            migrationBuilder.CreateIndex(
                name: "UX_rules_RuleGuidId_Version",
                table: "rules",
                columns: new[] { "RuleGuidId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_rules_Workflow",
                table: "workflow_rules",
                columns: new[] { "WorkflowId", "WorkflowVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_rules_Workflow_RuleGuidId_RuleVersion",
                table: "workflow_rules",
                columns: new[] { "WorkflowId", "RuleGuidId", "RuleVersion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_rules");

            migrationBuilder.DropIndex(
                name: "IX_rules_RuleGuidId_IsActive_Version",
                table: "rules");

            migrationBuilder.DropIndex(
                name: "UX_rules_RuleGuidId_Active",
                table: "rules");

            migrationBuilder.DropIndex(
                name: "UX_rules_RuleGuidId_Version",
                table: "rules");

            migrationBuilder.DropColumn(
                name: "RuleGuidId",
                table: "rules");
        }
    }
}

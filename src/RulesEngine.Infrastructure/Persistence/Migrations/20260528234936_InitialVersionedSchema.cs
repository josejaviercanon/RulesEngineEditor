using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RulesEngine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialVersionedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS rule_execution_states (
                    "Id" uuid PRIMARY KEY,
                    "WorkflowId" uuid NOT NULL,
                    "IsDryRun" boolean NOT NULL,
                    "WasSuccessful" boolean NOT NULL,
                    "ExecutedAtUtc" timestamp with time zone NOT NULL,
                    "ResultJson" text NOT NULL,
                    "ErrorJson" text NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS rules (
                    "Id" uuid PRIMARY KEY,
                    "Name" character varying(256) COLLATE pg_catalog."default" NOT NULL,
                    "Expression" character varying(1024) COLLATE pg_catalog."default" NOT NULL,
                    "RuleJson" text COLLATE pg_catalog."default" NOT NULL,
                    "Version" integer NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "EffectiveFromUtc" timestamp with time zone NULL,
                    "EffectiveToUtc" timestamp with time zone NULL
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS workflows (
                    "Id" uuid NOT NULL,
                    "Version" integer NOT NULL,
                    "Name" character varying(256) NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "EffectiveFromUtc" timestamp with time zone NULL,
                    "EffectiveToUtc" timestamp with time zone NULL,
                    "Definition" jsonb NOT NULL,
                    CONSTRAINT "PK_workflows" PRIMARY KEY ("Id", "Version")
                );
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'workflows'
                          AND column_name = 'Version') THEN
                        UPDATE workflows
                        SET "Version" = 1
                        WHERE "Version" IS NULL OR "Version" < 1;

                        WITH ranked AS (
                            SELECT ctid,
                                   ROW_NUMBER() OVER (
                                       PARTITION BY "Id"
                                       ORDER BY CASE WHEN "IsActive" THEN 0 ELSE 1 END,
                                                "Version" DESC
                                   ) AS rn
                            FROM workflows
                        )
                        UPDATE workflows AS workflow
                        SET "IsActive" = ranked.rn = 1
                        FROM ranked
                        WHERE workflow.ctid = ranked.ctid;

                        ALTER TABLE workflows DROP CONSTRAINT IF EXISTS "PK_workflows";
                        ALTER TABLE workflows ADD CONSTRAINT "PK_workflows" PRIMARY KEY ("Id", "Version");
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_workflows_Id_Version\";");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"UX_workflows_Id_Version\" ON workflows (\"Id\", \"Version\");");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_workflows_Id_Active\";");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS \"UX_workflows_Id_Active\" ON workflows (\"Id\") WHERE \"IsActive\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_workflows_Id_Active\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"UX_workflows_Id_Version\";");
            migrationBuilder.Sql("DROP TABLE IF EXISTS rule_execution_states;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS rules;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS workflows;");
        }
    }
}

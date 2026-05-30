using Microsoft.EntityFrameworkCore;
using RulesEngine.Core.Models;
using RulesEngine.Infrastructure.Persistence.Entities;
using PersistenceExecutionStateRecord = RulesEngine.Infrastructure.Persistence.Entities.ExecutionStateRecord;

namespace RulesEngine.Infrastructure.Persistence;

public sealed class RulesEngineEditorDbContext(DbContextOptions<RulesEngineEditorDbContext> options)
    : DbContext(options)
{
    public DbSet<RuleRecord> Rules => Set<RuleRecord>();
    public DbSet<WorkflowEntity> Workflows => Set<WorkflowEntity>();
    public DbSet<WorkflowRuleCollectionEntity> WorkflowRules => Set<WorkflowRuleCollectionEntity>();
    public DbSet<PersistenceExecutionStateRecord> ExecutionStates => Set<PersistenceExecutionStateRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RuleRecord>(entity =>
        {
            entity.ToTable("rules");

            entity.HasKey(rule => rule.Id)
                .HasName("PK_rules");

            entity.Property(rule => rule.Id)
                .HasColumnName("Id");

            entity.Property(rule => rule.Name)
                .HasColumnName("Name")
                .HasColumnType("character varying(256)")
                .UseCollation("pg_catalog.\"default\"")
                .IsRequired();

            entity.Property(rule => rule.RuleGuidId)
                .HasColumnName("RuleGuidId")
                .IsRequired();

            entity.Property(rule => rule.Expression)
                .HasColumnName("Expression")
                .HasColumnType("character varying(1024)")
                .UseCollation("pg_catalog.\"default\"")
                .IsRequired();

            entity.Property(rule => rule.ExecuteOrder)
                .HasColumnName("ExecuteOrder")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(rule => rule.RuleJson)
                .HasColumnName("RuleJson")
                .HasColumnType("text")
                .UseCollation("pg_catalog.\"default\"")
                .IsRequired();

            entity.Property(rule => rule.Version)
                .HasColumnName("Version")
                .IsRequired();

            entity.Property(rule => rule.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            entity.Property(rule => rule.Status)
                .HasColumnName("Status")
                .HasColumnType("character varying(32)")
                .HasConversion(
                    value => RuleStatusParser.ToValue(value),
                    value => RuleStatusParser.ParseOrDefault(value))
                .HasDefaultValue(RuleStatus.Draft)
                .IsRequired();

            entity.Property(rule => rule.EffectiveFromUtc)
                .HasColumnName("EffectiveFromUtc")
                .HasColumnType("timestamp with time zone");

            entity.Property(rule => rule.EffectiveToUtc)
                .HasColumnName("EffectiveToUtc")
                .HasColumnType("timestamp with time zone");

            entity.HasIndex(rule => new { rule.RuleGuidId, rule.Version })
                .IsUnique()
                .HasDatabaseName("UX_rules_RuleGuidId_Version");

            entity.HasIndex(rule => rule.RuleGuidId)
                .IsUnique()
                .HasFilter("\"IsActive\"")
                .HasDatabaseName("UX_rules_RuleGuidId_Active");

            entity.HasIndex(rule => new { rule.RuleGuidId, rule.IsActive, rule.Version })
                .HasDatabaseName("IX_rules_RuleGuidId_IsActive_Version");

            entity.ToTable(tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_rules_Status",
                    "\"Status\" IN ('draft', 'failed', 'disabled', 'production')"));
        });

        modelBuilder.Entity<WorkflowRuleCollectionEntity>(entity =>
        {
            entity.ToTable("workflow_rules");

            entity.HasKey(rule => new { rule.WorkflowId, rule.WorkflowVersion, rule.RuleGuidId })
                .HasName("PK_workflow_rules");

            entity.Property(rule => rule.WorkflowId)
                .HasColumnName("WorkflowId")
                .IsRequired();

            entity.Property(rule => rule.WorkflowVersion)
                .HasColumnName("WorkflowVersion")
                .IsRequired();

            entity.Property(rule => rule.RuleGuidId)
                .HasColumnName("RuleGuidId")
                .IsRequired();

            entity.Property(rule => rule.RuleVersion)
                .HasColumnName("RuleVersion")
                .IsRequired();

            entity.HasOne<WorkflowEntity>()
                .WithMany()
                .HasForeignKey(rule => new { rule.WorkflowId, rule.WorkflowVersion })
                .HasPrincipalKey(workflow => new { workflow.Id, workflow.Version })
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(rule => new { rule.WorkflowId, rule.WorkflowVersion })
                .HasDatabaseName("IX_workflow_rules_Workflow");

            entity.HasIndex(rule => new { rule.WorkflowId, rule.RuleGuidId, rule.RuleVersion })
                .HasDatabaseName("IX_workflow_rules_Workflow_RuleGuidId_RuleVersion");
        });

        modelBuilder.Entity<PersistenceExecutionStateRecord>(entity =>
        {
            entity.ToTable("rule_execution_states");

            entity.HasKey(state => state.Id)
                .HasName("PK_rule_execution_states");

            entity.Property(state => state.Id)
                .HasColumnName("Id");

            entity.Property(state => state.WorkflowId)
                .HasColumnName("WorkflowId")
                .IsRequired();

            entity.Property(state => state.IsDryRun)
                .HasColumnName("IsDryRun")
                .IsRequired();

            entity.Property(state => state.WasSuccessful)
                .HasColumnName("WasSuccessful")
                .IsRequired();

            entity.Property(state => state.ExecutedAtUtc)
                .HasColumnName("ExecutedAtUtc")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(state => state.ResultJson)
                .HasColumnName("ResultJson")
                .HasColumnType("text")
                .IsRequired();

            entity.Property(state => state.ErrorJson)
                .HasColumnName("ErrorJson")
                .HasColumnType("text");
        });

        modelBuilder.Entity<WorkflowEntity>(entity =>
        {
            entity.ToTable("workflows");

            entity.HasKey(workflow => new { workflow.Id, workflow.Version })
                .HasName("PK_workflows");

            entity.Property(workflow => workflow.Id)
                .HasColumnName("Id");

            entity.Property(workflow => workflow.Version)
                .HasColumnName("Version")
                .IsRequired();

            entity.Property(workflow => workflow.Name)
                .HasColumnName("Name")
                .HasColumnType("character varying(256)")
                .IsRequired();

            entity.Property(workflow => workflow.WorkflowJson)
                .HasColumnName("WorkflowJson")
                .HasColumnType("text");

            entity.Property(workflow => workflow.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            entity.Property(workflow => workflow.IsEnabled)
                .HasColumnName("IsEnabled")
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(workflow => workflow.Comments)
                .HasColumnName("Comments")
                .HasColumnType("character varying(4000)");

            entity.Property(workflow => workflow.EffectiveFromUtc)
                .HasColumnName("EffectiveFromUtc")
                .HasColumnType("timestamp with time zone");

            entity.Property(workflow => workflow.EffectiveToUtc)
                .HasColumnName("EffectiveToUtc")
                .HasColumnType("timestamp with time zone");

            entity.OwnsOne(workflow => workflow.Definition, definition =>
            {
                definition.ToJson("Definition");
                definition.Property(value => value.Expression).HasJsonPropertyName("Expression");
                definition.Property(value => value.RuleJson).HasJsonPropertyName("RuleJson");
            });

            entity.HasIndex(workflow => new { workflow.Id, workflow.Version })
                .IsUnique()
                .HasDatabaseName("UX_workflows_Id_Version");

            entity.HasIndex(workflow => workflow.Id)
                .IsUnique()
                .HasFilter("\"IsActive\"")
                .HasDatabaseName("UX_workflows_Id_Active");

            entity.HasIndex(workflow => new { workflow.Id, workflow.IsEnabled })
                .IsUnique()
                .HasFilter("\"IsActive\"")
                .HasDatabaseName("UX_workflows_Id_ActiveEnabled");
        });
    }
}

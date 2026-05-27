using Microsoft.EntityFrameworkCore;
using RulesEngine.Infrastructure.Persistence.Entities;

namespace RulesEngine.Infrastructure.Persistence;

public sealed class RulesEngineEditorDbContext(DbContextOptions<RulesEngineEditorDbContext> options)
    : DbContext(options)
{
    public DbSet<RuleRecord> Rules => Set<RuleRecord>();
    public DbSet<WorkflowEntity> Workflows => Set<WorkflowEntity>();
    public DbSet<ExecutionStateRecord> ExecutionStates => Set<ExecutionStateRecord>();

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

            entity.Property(rule => rule.Expression)
                .HasColumnName("Expression")
                .HasColumnType("character varying(1024)")
                .UseCollation("pg_catalog.\"default\"")
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

            entity.Property(rule => rule.EffectiveFromUtc)
                .HasColumnName("EffectiveFromUtc")
                .HasColumnType("timestamp with time zone");

            entity.Property(rule => rule.EffectiveToUtc)
                .HasColumnName("EffectiveToUtc")
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<ExecutionStateRecord>(entity =>
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

            entity.HasKey(workflow => workflow.Id)
                .HasName("PK_workflows");

            entity.Property(workflow => workflow.Id)
                .HasColumnName("Id");

            entity.Property(workflow => workflow.Name)
                .HasColumnName("Name")
                .HasColumnType("character varying(256)")
                .IsRequired();

            entity.Property(workflow => workflow.Version)
                .HasColumnName("Version")
                .IsRequired();

            entity.Property(workflow => workflow.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            entity.Property(workflow => workflow.EffectiveFromUtc)
                .HasColumnName("EffectiveFromUtc")
                .HasColumnType("timestamp with time zone");

            entity.Property(workflow => workflow.EffectiveToUtc)
                .HasColumnName("EffectiveToUtc")
                .HasColumnType("timestamp with time zone");

            entity.OwnsOne(workflow => workflow.Definition, definition =>
            {
                definition.ToJson("Definition");
                definition.Property(value => value.Expression).HasColumnName("Expression");
                definition.Property(value => value.RuleJson).HasColumnName("RuleJson");
            });
        });
    }
}

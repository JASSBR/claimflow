using Microsoft.EntityFrameworkCore;

namespace ClaimFlow.BuildingBlocks.Outbox;

public static class OutboxModelBuilderExtensions
{
    /// <summary>Each module owns its outbox table inside its own schema: modules never share tables.</summary>
    public static ModelBuilder ApplyOutbox(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(outbox =>
        {
            outbox.ToTable("outbox_messages");
            outbox.HasKey(message => message.Id);
            outbox.Property(message => message.Type).HasMaxLength(256);
            outbox.Property(message => message.Payload).HasColumnType("jsonb");
            outbox.Property(message => message.LastError).HasMaxLength(2000);
            // Partial index: the processor only ever scans pending rows, which stay a tiny fraction of the table.
            outbox.HasIndex(message => message.OccurredAt).HasFilter("\"ProcessedAt\" IS NULL");
        });

        return modelBuilder;
    }
}

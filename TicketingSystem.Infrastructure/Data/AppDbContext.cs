using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly (if using IEntityTypeConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureUser(modelBuilder);
        ConfigureTicket(modelBuilder);
        ConfigureTicketComment(modelBuilder);
        ConfigureTicketAttachment(modelBuilder);
        ConfigureTicketHistory(modelBuilder);
    }

    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .IsRequired();
        });
    }

    private void ConfigureTicket(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.Description)
                .IsRequired();

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.Priority)
                .IsRequired();

            // CreatedBy (User)
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // AssignedTo (User)
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comments
            entity.HasMany(x => x.Comments)
                .WithOne()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // Attachments
            entity.HasMany(x => x.Attachments)
                .WithOne()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            // History
            entity.HasMany(x => x.History)
                .WithOne()
                .HasForeignKey(x => x.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureTicketComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTicketAttachment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(x => x.FilePath)
                .IsRequired();
        });
    }

    private void ConfigureTicketHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketHistory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PropertyChanged)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.OldValue)
                .HasMaxLength(1000);

            entity.Property(x => x.NewValue)
                .HasMaxLength(1000);

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    // Automatically update UpdatedAt
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

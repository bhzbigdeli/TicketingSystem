using TicketingSystem.Domain.Enums;

namespace TicketingSystem.Domain.Entities;

public class Ticket : BaseEntity
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TicketStatus Status { get; private set; }
    public TicketPriority Priority { get; private set; }

    public Guid CreatedById { get; private set; }
    public Guid? AssignedToId { get; private set; }

    public ICollection<TicketComment> Comments { get; private set; } = new List<TicketComment>();
    public ICollection<TicketAttachment> Attachments { get; private set; } = new List<TicketAttachment>();
    public ICollection<TicketHistory> History { get; private set; } = new List<TicketHistory>();

    private Ticket() { } // EF Core

    public Ticket(string title, string description, TicketPriority priority, Guid createdById)
    {
        Title = title;
        Description = description;
        Priority = priority;
        CreatedById = createdById;
        Status = TicketStatus.Open;
    }

    public void Assign(Guid userId)
    {
        AssignedToId = userId;
        Status = TicketStatus.InProgress;
        SetUpdated();
    }

    public void ChangeStatus(TicketStatus status)
    {
        Status = status;
        SetUpdated();
    }

    public void UpdateDetails(string title, string description, TicketPriority priority)
    {
        Title = title;
        Description = description;
        Priority = priority;
        SetUpdated();
    }

    public void Close()
    {
        Status = TicketStatus.Closed;
        SetUpdated();
    }
}


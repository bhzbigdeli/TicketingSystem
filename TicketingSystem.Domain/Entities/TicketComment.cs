namespace TicketingSystem.Domain.Entities;

public class TicketComment : BaseEntity
{
    public string Content { get; private set; }

    public Guid TicketId { get; private set; }
    public Guid UserId { get; private set; }

    private TicketComment() { }

    public TicketComment(string content, Guid ticketId, Guid userId)
    {
        Content = content;
        TicketId = ticketId;
        UserId = userId;
    }
    public void UpdateContent(string content)
    {
        Content = content;
        SetUpdated();
    }
}

namespace TicketingSystem.Domain.Entities;

public class TicketHistory : BaseEntity
{
    public Guid TicketId { get; private set; }
    public Guid ChangedById { get; private set; }
    public string PropertyChanged { get; private set; }
    public string OldValue { get; private set; }
    public string NewValue { get; private set; }

    private TicketHistory() { }

    public TicketHistory(Guid ticketId, Guid changedById,
        string propertyChanged, string oldValue, string newValue)
    {
        TicketId = ticketId;
        ChangedById = changedById;
        PropertyChanged = propertyChanged;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

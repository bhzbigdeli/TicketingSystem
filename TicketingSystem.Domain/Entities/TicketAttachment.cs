namespace TicketingSystem.Domain.Entities;

public class TicketAttachment : BaseEntity
{
    public string FileName { get; private set; }
    public string FilePath { get; private set; }

    public Guid TicketId { get; private set; }

    private TicketAttachment() { }

    public TicketAttachment(string fileName, string filePath, Guid ticketId)
    {
        FileName = fileName;
        FilePath = filePath;
        TicketId = ticketId;
    }
}

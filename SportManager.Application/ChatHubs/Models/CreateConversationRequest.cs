namespace SportManager.Application.ChatHubs.Models;

public class CreateConversationRequest
{
    public List<Guid> Participants { get; set; } = new();
    public string? Title { get; set; }
}

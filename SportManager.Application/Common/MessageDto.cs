using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SportManager.Domain.Constants;

namespace SportManager.Application.ChatHubs.Models;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; }
    public MessageStatus MessageStatus { get; set; }
    public MessageType MessageType { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}


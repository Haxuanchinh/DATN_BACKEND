using Microsoft.VisualBasic;
using SportManager.Domain.Constants;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportManager.Domain.Entity;

[Table(TableNameConstants.Conversation)]
public class Conversation : EntityBase<Guid>
{
    public string Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
}

[Table(TableNameConstants.ConversationParticipant)]
public class ConversationParticipant : EntityBase<Guid>
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; } // Đánh dấu nếu người tham gia là admin
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }

    public virtual Conversation Conversation { get; set; }
    public virtual User User { get; set; }
}

[Table(TableNameConstants.Message)]
public class Message : EntityBase<Guid>
{
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsSystemMessage { get; set; } // Đánh dấu tin nhắn hệ thống (ví dụ: "Admin đã tham gia cuộc trò chuyện")
    public MessageType MessageType { get; set; }
    public virtual Conversation Conversation { get; set; }
    public virtual User Sender { get; set; }
    public virtual ICollection<MessageStatus> MessageStatuses { get; set; } = new List<MessageStatus>();
}

[Table(TableNameConstants.MessageStatus)]
public class MessageStatus : EntityBase<Guid>
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual Message Message { get; set; }
    public virtual User User { get; set; }
}
using Microsoft.AspNetCore.SignalR;
using SportManager.Application.Abstractions;
using SportManager.Application.ChatHubs.Commands;
using SportManager.Application.Common.Interfaces;
using SportManager.Domain.Constants;
using SportManager.Domain.Entity;

public class ChatHub : Hub
{
    private readonly ICurrentUserService _userService;
    private readonly IApplicationDbContext _dbContext;
    private readonly UserConnectionManager _connectionManager;

    public ChatHub(ICurrentUserService userService, IApplicationDbContext dbContext, UserConnectionManager connectionManager)
    {
        _userService = userService;
        _dbContext = dbContext;
        _connectionManager = connectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            Console.WriteLine($"Client connecting. ConnectionId: {Context.ConnectionId}");

            var userId = _userService.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                // Join group theo userId để nhận các sự kiện cá nhân (ConversationCreated, v.v.)
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            // Join tất cả các group conversation mà user là thành viên
            var userGuid = Guid.Parse(userId);
            var conversations = await _dbContext.ConversationParticipants
                .Where(cp => cp.UserId == userGuid)
                .Select(cp => cp.ConversationId)
                .ToListAsync();

            foreach (var conversationId in conversations)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            }

            Console.WriteLine($"User authenticated. UserId: {userId}");
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in OnConnectedAsync: {ex.Message}");
            throw;
        }
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = _userService.UserId;
            Console.WriteLine($"User {userId} disconnected. Connection ID: {Context.ConnectionId}");

            if (!string.IsNullOrEmpty(Context.ConnectionId))
            {
                _connectionManager.RemoveConnection(Guid.Parse(userId), Context.ConnectionId);
            }
            else
            {
                Console.WriteLine("Connection ID is null or empty.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during OnDisconnectedAsync: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid conversationId, string content, MessageType messageType = 0)
    {
        var userId = Guid.Parse(_userService.UserId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new HubException("Người dùng không tồn tại.");
        }

        var conversationParticipant = await _dbContext.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

        if (conversationParticipant == null)
        {
            throw new HubException("Bạn không có quyền gửi tin nhắn trong cuộc trò chuyện này.");
        }


        var message = new Message
        {
            MessageType = messageType,
            ConversationId = conversationId,
            SenderId = userId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"Sending message to group {conversationId}");
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", message);
    }


    public async Task SendImages(Guid conversationId, List<string> imageUrls)
    {
        var userId = Guid.Parse(_userService.UserId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new HubException("Người dùng không tồn tại.");
        }

        var messages = imageUrls.Select(url => new Message
        {
            MessageType = MessageType.Image,
            ConversationId = conversationId,
            SenderId = userId,
            Content = url,
            SentAt = DateTime.UtcNow
        }).ToList();

        await _dbContext.Messages.AddRangeAsync(messages);
        await _dbContext.SaveChangesAsync();

        foreach (var message in messages)
        {
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", message);
        }
    }


    public async Task CreateConversation(string title, List<Guid> participantIds)
    {
        var userId = Guid.Parse(_userService.UserId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new HubException("Người dùng không tồn tại.");
        }

        bool isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");

        if (!participantIds.Contains(userId))
        {
            participantIds.Add(userId);
        }

        var conversation = new Conversation
        {
            Title = title,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        foreach (var participantId in participantIds)
        {
            var participant = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = participantId,
                IsAdmin = isAdmin
            };
            _dbContext.ConversationParticipants.Add(participant);
        }
        await _dbContext.SaveChangesAsync();

        foreach (var participantId in participantIds)
        {
            var connections = _connectionManager.GetUserConnections(participantId);
            foreach (var connectionId in connections)
            {
                await Groups.AddToGroupAsync(connectionId, conversation.Id.ToString());
            }
        }

        try
        {
            await Clients.Group(conversation.Id.ToString()).SendAsync("ConversationCreated", new
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                CreatedBy = userId,
                Participants = participantIds.Select(id => new
                {
                    UserId = id,
                    Username = _dbContext.Users.FirstOrDefault(u => u.Id == id)?.Username
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending ConversationCreated event: {ex.Message}");
        }

    }

    public async Task HandleReceivedMessage(Guid conversationId, string messageContent)
    {
        Console.WriteLine($"Message received in conversation {conversationId}: {messageContent}");
        await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new
        {
            Content = messageContent,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task JoinConversation(Guid conversationId)
    {
        try
        {
            var userId = Guid.Parse(_userService.UserId);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new HubException("Người dùng không tồn tại.");
            }

            // Kiểm tra người dùng có phải là thành viên của cuộc trò chuyện không
            var conversationParticipant = await _dbContext.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (conversationParticipant == null)
            {
                throw new HubException("Bạn không phải là thành viên của cuộc trò chuyện này.");
            }

            // Thêm người dùng vào nhóm (group) của cuộc trò chuyện
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            Console.WriteLine($"User {userId} joined conversation {conversationId}");

            // Có thể gửi lại thông tin cuộc trò chuyện cho client nếu cần
            await Clients.Caller.SendAsync("ConversationJoined", new { ConversationId = conversationId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinConversation: {ex.Message}");
            throw new HubException("Lỗi khi tham gia cuộc trò chuyện.");
        }
    }

}

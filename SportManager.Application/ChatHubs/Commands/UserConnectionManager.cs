using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportManager.Application.ChatHubs.Commands;

public class UserConnectionManager
{
    private readonly Dictionary<Guid, List<string>> _userConnections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        if (!_userConnections.ContainsKey(userId))
        {
            _userConnections[userId] = new List<string>();
        }

        _userConnections[userId].Add(connectionId);
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (_userConnections.ContainsKey(userId))
        {
            _userConnections[userId].Remove(connectionId);

            if (_userConnections[userId].Count == 0)
            {
                _userConnections.Remove(userId);
            }
        }
    }

    public List<string> GetUserConnections(Guid userId)
    {
        if (_userConnections.ContainsKey(userId))
        {
            return _userConnections[userId];
        }

        return new List<string>();
    }
}

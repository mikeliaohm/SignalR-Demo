using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using signalr.Services;

namespace signalr.Hubs
{
    public class MessageHub : Hub
    {
        private IActiveUserCollection _activeUserCollection;

        public MessageHub(IActiveUserCollection activeUserCollection)
        {
            _activeUserCollection = activeUserCollection;
        }

        public Task SendMessageToAll(string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", message);
        }

        public Task SendMessageToCaller(string message)
        {
            return Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public Task JoinGroup(string group)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public Task SendMessageToGroup(string group, string message)
        {
            return Clients.Group(group).SendAsync("ReceiveMessage", message);
        }

        public Task SendMessageToUser(string connectionId, string message)
        {
            return Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }

        public Task SendNewUserLoginMessage()
        {
            var rand = new Random();
            var randomId = rand.Next(100);
            var newUser = 
                _activeUserCollection.NewUserLoggedIn(
                    randomId, 
                    $"User {randomId}",
                    Context.ConnectionId
                );

            if (newUser != null)
                return Clients.All.SendAsync("NewUserLoggedIn", _activeUserCollection.ActiveUsers);
            
            return null;
        }

        public Task StoreNewUserLogin(int id, string username)
        {
            var newUser = _activeUserCollection.NewUserLoggedIn(id, username, Context.ConnectionId);

            if (newUser != null)
                return Clients.All.SendAsync("NewUserLoggedIn", _activeUserCollection.ActiveUsers);

            return null;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }
    }
}
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Guacamole.Mvc
{
    public sealed class GuacHub : Hub
    {
        private static readonly Dictionary<string, GuacClient> s_clients = new Dictionary<string, GuacClient>();

        public override System.Threading.Tasks.Task OnConnected()
        {
            lock (s_clients)
            {
                GuacClient client = new GuacClient(this.Context.ConnectionId);
                s_clients.Add(client.Id, client);
                client.Start();
            }
            return base.OnConnected();
        }

        public async Task<bool> Connect(int serverId, string[] audio, string[] video)
        {
            GuacClient client;
            lock (s_clients)
            {
                if (s_clients.ContainsKey(this.Context.ConnectionId))
                    client = s_clients[this.Context.ConnectionId];
                else
                    return false;
            }
            await client.Connect(serverId, audio, video);
            return true;
        }

        public async Task<bool> Send(string[] elements)
        {
            GuacClient client;
            lock (s_clients)
            {
                if (s_clients.ContainsKey(this.Context.ConnectionId))
                    client = s_clients[this.Context.ConnectionId];
                else
                    return false;
            }
            await client.Send(elements);
            return true;
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            lock (s_clients)
            {
                if (s_clients.ContainsKey(this.Context.ConnectionId))
                {
                    using (GuacClient client = s_clients[this.Context.ConnectionId])
                    {
                        s_clients.Remove(client.Id);
                    }
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Guacamole.Mvc
{
    internal sealed class GuacClient : IDisposable
    {
        private readonly string m_id;
        private readonly Socket m_socket;
        private NetworkStream m_readStream;
        private BufferedStream m_writeStream;
        //private readonly Thread m_thRead;
        private readonly byte[] m_buffer;
        private bool m_run = true;
        private readonly BlockingCollection<string[]> m_readQueue;

        public string Id { get { return this.m_id; } }

        internal GuacClient(string id)
        {
            this.m_id = id;

            this.m_buffer = new byte[1 << 20]; //1Mb
            this.m_socket = new Socket(SocketType.Stream, ProtocolType.IP);
            this.m_readQueue = new BlockingCollection<string[]>();
        }

        /// <summary>
        /// Start reading
        /// </summary>
        internal void Start()
        {
            DnsEndPoint ep = new DnsEndPoint(ConfigurationManager.AppSettings["GuacD:Host"], int.Parse(ConfigurationManager.AppSettings["GuacD:Port"]));
            this.m_socket.Connect(ep);
            this.m_readStream = new NetworkStream(this.m_socket);
            this.m_writeStream = new BufferedStream(this.m_readStream);
            Task.Run((Func<Task>)this.ReadPump);
        }

        public void Dispose()
        {
            using (this.m_socket)
            {
                this.m_run = false;
                this.m_writeStream.Dispose();
                this.m_readStream.Dispose();
            }
        }

        private async Task ReadPump()
        {
            try
            {
                while (this.m_run)
                {
                    string command = await GuaClientExtensions.ReadPartAsync(this.m_readStream);
                    bool loop = true;
                    List<string> args = new List<string>();
                    do
                    {
                        char sep = await GuaClientExtensions.ReadSepAsync(this.m_readStream);
                        switch (sep)
                        {
                            case ',':
                                args.Add(await GuaClientExtensions.ReadPartAsync(this.m_readStream));
                                break;
                            case ';':
                                this.HasRead(command, args.ToArray());
                                loop = false;
                                break;
                            default:
                                System.Diagnostics.Debugger.Break();
                                return;
                        }
                    }
                    while (loop);
                }
            }
            catch (IOException)
            {

            }
            catch (ObjectDisposedException)
            {

            }
        }

        private void HasRead(string command, string[] args)
        {
            //this.m_readQueue.Add(new GuacMessage() { Command = command, Args = args.ToArray() });
            List<string> elements = new List<string>(args.Length + 1);
            elements.Add(command);
            elements.AddRange(args);
            this.m_readQueue.Add(elements.ToArray());
        }

        internal async Task Send(string[] elements)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(elements[0].Length.ToString(CultureInfo.InvariantCulture));
            sb.Append(".");
            sb.Append(elements[0]);
            for (int i = 1; i < elements.Length; i++)
            {
                sb.Append(",");
                sb.Append(elements[i].Length.ToString(CultureInfo.InvariantCulture));
                sb.Append(".");
                sb.Append(elements[i]);
            }
            sb.Append(";");
            byte[] arr = Encoding.UTF8.GetBytes(sb.ToString());
            await this.m_writeStream.WriteAsync(arr, 0, arr.Length);
            await this.m_writeStream.FlushAsync();
        }

        internal async Task Connect(int serverId, string[] audio, string[] video)
        {
            switch (serverId)
            {
                case 42:
                    await this.ConnectRdp(audio, video);
                    break;
                case 33:
                    await this.ConnectSSH(audio, video);
                    break;
                default:
                    throw new NotImplementedException();
            }

            Task.Run((Func<Task>)this.SendToClient);
        }

        private async Task ConnectRdp(string[] audio, string[] video)
        {
            await this.m_writeStream.Select("rdp");
            string[] args = await this.m_readQueue.TakeAsync();
            await this.m_writeStream.Size(1024, 768, 96);
            await this.m_writeStream.WriteInstruction("audio", audio);
            await this.m_writeStream.WriteInstruction("video", video);
            System.Diagnostics.Debug.WriteLine("Args to send : " + (args.Length - 1));
            await this.m_writeStream.Connect_Rdp(
                hostname: "192.168.1.2",
                port: 3389,
                width: 1024,
                height: 768,
                color_depth: 16,
                disable_auth: true,
                disable_audio: true,
                //server_layout: "fr-fr-azerty"
                server_layout: "failsafe"
                );
        }

        private async Task ConnectSSH(string[] audio, string[] video)
        {
            await this.m_writeStream.Select("ssh");
            string[] args = await this.m_readQueue.TakeAsync();
            await this.m_writeStream.Size(1024, 768, 96);
            await this.m_writeStream.WriteInstruction("audio", audio);
            await this.m_writeStream.WriteInstruction("video", video);
            System.Diagnostics.Debug.WriteLine("Args to send : " + (args.Length - 1));
            await this.m_writeStream.Connect_Ssh(
                hostname: "192.168.1.9",
                port: 22);
        }

        private async Task SendToClient()
        {
            string[] elements = await this.m_readQueue.TakeAsync();
            if (elements != null && this.m_run)
            {
                await GlobalHost.ConnectionManager.GetHubContext<GuacHub>().Clients.Client(this.Id).Receive(elements);
                Task.Run((Func<Task>)this.SendToClient);
            }
        }
    }
}
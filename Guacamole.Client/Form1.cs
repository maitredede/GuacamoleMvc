using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Guacamole.Client
{
    public partial class Form1 : Form
    {
        private byte[] m_readBuffer;
        private readonly Socket m_tcp;
        private NetworkStream m_ns;
        private BufferedStream m_buffer;
        private readonly BlockingCollection<Message> m_readQueue;
        private Thread m_thPump;

        private Bitmap m_draw;

        public Form1()
        {
            this.InitializeComponent();
            this.m_tcp = new Socket(SocketType.Stream, ProtocolType.IP);
            this.m_readBuffer = new byte[1 << 20];
            this.m_readQueue = new BlockingCollection<Message>();
        }

        private struct Message
        {
            public string Command { get; set; }
            public string[] Args { get; set; }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //await this.m_ws.ConnectAsync(new Uri("ws://192.168.1.12:4882"), CancellationToken.None);
            //await this.m_ws.
            this.m_tcp.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.12"), 4822));
            this.m_ns = new NetworkStream(this.m_tcp);
            this.m_buffer = new BufferedStream(this.m_ns);
            ThreadPool.QueueUserWorkItem((o) => this.m_ns.StartReadItem(this.m_readBuffer, this.ItemRead, this.ReadError));
            //this.m_ns.BeginRead(this.m_readBuffer, 0, this.m_readBuffer.Length, this.EndRead, null);

            this.HandShake();
        }

        private void ItemRead(string command, string[] args)
        {
            Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
            this.m_readQueue.Add(new Message() { Command = command, Args = args });
        }

        private void ReadError(string error)
        {

        }

        //private void EndRead(IAsyncResult ar)
        //{
        //    int read = this.m_ns.EndRead(ar);
        //    if (read > 0)
        //    {
        //        string str = Encoding.UTF8.GetString(this.m_readBuffer, 0, read);
        //        Console.WriteLine(str);
        //        this.m_readQueue.Add(str);
        //        this.m_ns.BeginRead(this.m_readBuffer, 0, this.m_readBuffer.Length, this.EndRead, null);
        //    }
        //}

        private void btnHandShake_Click(object sender, EventArgs e)
        {

        }

        private async Task HandShake()
        {
            this.btnHandShake.Enabled = false;

            //Handshake
            await this.m_buffer.Select("rdp");
            Message args = await this.m_readQueue.TakeAsync();

            //select response
            await this.m_buffer.Size(1024, 768, 96);
            await this.m_buffer.WriteInstruction("audio");
            await this.m_buffer.WriteInstruction("video");
            //await this.m_buffer.WriteInstruction("video", "image/png");
            //4.args,8.hostname,4.port,6.domain,8.username,8.password,5.width,6.height,15.initial-program,11.color-depth,13.disable-audio,15.enable-printing,7.console,13.console-audio,13.server-layout,8.security,11.ignore-cert,12.disable-auth;
            //await this.m_buffer.WriteInstruction("connect", "192.168.1.2", 3389, string.Empty, string.Empty, string.Empty);

            Debug.WriteLine("Args to send : " + args.Args.Length);
            await this.m_buffer.Connect_Rdp(hostname: "192.168.1.2", port: 3389, width: this.pan.Width, height: this.pan.Height, color_depth: 16, disable_auth: true);

            this.m_thPump = new Thread(this.Pump);
            this.m_thPump.Name = "Pump";
            this.m_thPump.Start();
        }

        private void Pump()
        {
            while (true)
            {
                Message response = this.m_readQueue.Take();
                this.BeginInvoke(new Action<Message>(this.Execute), response);
            }
        }

        private void Execute(Message msg)
        {
            switch (msg.Command)
            {
                case "name":
                    this.Text = msg.Args[0];
                    break;
                case "size":
                    this.pan.Width = int.Parse(msg.Args[1]);
                    this.pan.Height = int.Parse(msg.Args[2]);
                    this.m_draw = new Bitmap(this.pan.Width, this.pan.Height);
                    this.pan.BackgroundImage = this.m_draw;
                    break;
                case "png":
                    this.Execute_png(msg.Args[0], msg.Args[1], int.Parse(msg.Args[2]), int.Parse(msg.Args[3]), Convert.FromBase64String(msg.Args[4]));
                    break;
                case "cursor":
                    break;
                default:
                    Debug.WriteLine(string.Format("Not implemented : Execute : {0} + {1} args", msg.Command, msg.Args.Length));
                    break;
            }
        }

        private void Execute_png(string mask, string layer, int x, int y, byte[] data)
        {
            Image png;
            using (MemoryStream ms = new MemoryStream(data))
            {
                png = Image.FromStream(ms);
            }

            using (Graphics g = Graphics.FromImage(this.m_draw))
            {
                g.DrawImageUnscaled(png, x, y);
            }
            //this.pan.Invalidate();
        }
    }
}

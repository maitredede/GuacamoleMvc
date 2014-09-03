using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    public sealed class GuacClient : SynchronizedDataItem, IDisposable
    {
        private readonly IPEndPoint m_server;
        private readonly byte[] m_readBuffer;
        private readonly Socket m_tcp;
        private NetworkStream m_readStream;
        private BufferedStream m_writeStream;
        private readonly BlockingCollection<GuacMessage> m_readQueue;

        private readonly Thread m_readStreamPump;
        private readonly Thread m_handleMessagePump;

        public GuacClient(IPEndPoint server)
        {
            this.m_server = server;
            this.m_tcp = new Socket(SocketType.Stream, ProtocolType.IP);
            this.m_readBuffer = new byte[1 << 20];
            this.m_readQueue = new BlockingCollection<GuacMessage>();
            this.m_readStreamPump = new Thread(this.ReadStreamPump);
            this.m_readStreamPump.Name = "ReadStreamPump";
            this.m_handleMessagePump = new Thread(this.HandleMessagePump);
            this.m_handleMessagePump.Name = "HandleMessagePump";
        }

        public void Dispose()
        {
            using (this.m_tcp)
            {
                this.m_readStream.Dispose();
                this.m_writeStream.Dispose();
            }
        }

        public async Task Start(string hostname, int port, int width, int height, int color_depth)
        {
            await this.m_tcp.ConnectAsyncTask(this.m_server);
            this.m_readStream = new NetworkStream(this.m_tcp);
            this.m_writeStream = new BufferedStream(this.m_readStream);
            this.m_readStreamPump.Start();

            //handshake
            await this.m_writeStream.Select("rdp");
            GuacMessage args = await this.m_readQueue.TakeAsync();
            await this.m_writeStream.Size(1024, 768, 96);
            await this.m_writeStream.WriteInstruction("audio");
            await this.m_writeStream.WriteInstruction("video");
            System.Diagnostics.Debug.WriteLine("Args to send : " + args.Args.Length);
            await this.m_writeStream.Connect_Rdp(hostname: hostname, port: port, width: width, height: height, color_depth: color_depth, disable_auth: true);
            this.m_handleMessagePump.Start();
        }

        private void ReadStreamPump()
        {
            while (true)
            {
                string command = GuaClientExtensions.ReadPartAsync(this.m_readStream).Result;
                bool loop = true;
                List<string> args = new List<string>();
                do
                {
                    char sep = GuaClientExtensions.ReadSepAsync(this.m_readStream).Result;
                    switch (sep)
                    {
                        case ',':
                            args.Add(GuaClientExtensions.ReadPartAsync(this.m_readStream).Result);
                            break;
                        case ';':
                            this.m_readQueue.Add(new GuacMessage() { Command = command, Args = args.ToArray() });
                            loop = false;
                            break;
                        default:
                            System.Diagnostics.Debugger.Break();
                            return;
                    }
                } while (loop);
            }
        }

        private void HandleMessagePump()
        {
            while (true)
            {
                GuacMessage msg = this.m_readQueue.Take();
                switch (msg.Command)
                {
                    case "name":
                        this.Name = msg.Args[0];
                        break;
                    case "size":
                        this.ResizeLayer(int.Parse(msg.Args[0]), int.Parse(msg.Args[1]), int.Parse(msg.Args[2]));
                        break;
                    case "png":
                        this.DrawPng((Operation)int.Parse(msg.Args[0]), int.Parse(msg.Args[1]), int.Parse(msg.Args[2]), int.Parse(msg.Args[3]), Convert.FromBase64String(msg.Args[4]));
                        break;
                    case "cursor":
                        this.SetCursorData(int.Parse(msg.Args[0]), int.Parse(msg.Args[1]), int.Parse(msg.Args[2]), int.Parse(msg.Args[3]), int.Parse(msg.Args[4]), int.Parse(msg.Args[5]), int.Parse(msg.Args[6]));
                        break;
                    case "sync":
                        TimeSpan ts = TimeSpan.FromMilliseconds(ulong.Parse(msg.Args[0]));
                        this.m_writeStream.WriteInstruction("sync", ((ulong)(new DateTime(1970, 1, 1) - DateTime.Now).TotalMilliseconds)).Wait();
                        break;
                    case "copy":
                        this.Copy(int.Parse(msg.Args[0]), int.Parse(msg.Args[1]), int.Parse(msg.Args[2]), int.Parse(msg.Args[3]), int.Parse(msg.Args[4]), (Operation)int.Parse(msg.Args[5]), int.Parse(msg.Args[6]), int.Parse(msg.Args[7]), int.Parse(msg.Args[8]));
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }
        }

        private void Copy(int srclayer, int srcx, int srcy, int srcwidth, int srcheight, Operation mask, int dstlayer, int dstx, int dsty)
        {
            Layer sl = this.GetLayer(srclayer);
            Layer dl = this.GetLayer(dstlayer);
            dl.CopyFrom(sl, mask, srcx, srcy, srcwidth, srcheight, dstx, dsty);
        }

        private string m_name;
        public string Name
        {
            get { return this.m_name; }
            private set { this.Set(value, ref this.m_name); }
        }

        private readonly Dictionary<int, Layer> m_layers = new Dictionary<int, Layer>();
        private readonly Cursor m_cursor = new Cursor();

        private void ResizeLayer(int index, int width, int height)
        {
            Layer layer = this.GetLayer(index);

            layer.Resize(width, height);

            //if (this.m_layersBmp.ContainsKey(index))
            //{
            //    this.m_layersGraphics[index].Dispose();
            //    this.m_layersBmp[index].Dispose();
            //}
            //else
            //{
            //    this.m_layersBmp.Add(index, null);
            //    this.m_layersGraphics.Add(index, null);
            //}

            //this.m_layersBmp[index] = new Bitmap(width, height);
            //this.m_layersGraphics[index] = Graphics.FromImage(this.m_layersBmp[index]);
            //this.m_layersGraphics[index].Clear(Color.Transparent);

            this.RaisePropertyChanged("Layers");
            //TODO : Recopy old layer ?
        }

        private Layer GetLayer(int index)
        {
            Layer layer;
            if (!this.m_layers.ContainsKey(index))
            {
                if (index > 0)
                    layer = new BufferLayer(1, 1);
                else
                    layer = new VisibleLayer(1, 1);
                this.m_layers.Add(index, layer);
            }
            else
            {
                layer = this.m_layers[index];
            }
            return layer;
        }

        private void DrawPng(Operation mask, int layer, int x, int y, byte[] data)
        {
            Image png;
            using (MemoryStream ms = new MemoryStream(data))
            {
                png = Image.FromStream(ms);
            }
            Layer l = this.GetLayer(layer);
            l.DrawPng(mask, png, x, y);
            //using (png)
            //{
            //    if (layer == -1)
            //    {
            //        foreach (Graphics g in this.m_layersGraphics.Values)
            //        {
            //            this.DrawPng(g, mask, png, x, y);
            //        }
            //    }
            //    else
            //    {
            //        Graphics g = this.m_layersGraphics[layer];
            //        this.DrawPng(g, mask, png, x, y);
            //    }
            //}
        }

        private void SetCursorData(int x, int y, int srclayer, int srcx, int srcy, int srcwidth, int srcheight)
        {
            this.m_cursor.HotSpot = new Point(x, y);
            this.m_cursor.Layer = this.GetLayer(srclayer);
            this.m_cursor.Rect = new Rectangle(srcx, srcy, srcwidth, srcheight);
            //this.m_cursorHotSpot = new Point(x, y);
            //this.m_cursor = new Bitmap(srcwidth, srcheight);
            //this.m_cursorGraphics = Graphics.FromImage(this.m_cursor);

            //Rectangle srcRect = new Rectangle(srcx, srcy, srcwidth, srcheight);
            //Rectangle destRect = new Rectangle(0, 0, srcwidth, srcheight);
            //this.m_cursorGraphics.DrawImage(this.m_layersBmp[0], srcRect: srcRect, destRect: destRect, srcUnit: GraphicsUnit.Pixel);
        }

        public Dictionary<int, Layer> Layers { get { return this.m_layers; } }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Guacamole.Client
{
    public partial class Form2 : Form
    {
        private readonly GuacClient m_client;

        public Form2()
        {
            this.InitializeComponent();
            this.m_client = new GuacClient(new IPEndPoint(IPAddress.Parse("192.168.1.12"), 4822));
            this.m_client.PropertyChanged += m_client_PropertyChanged;

            this.DataBindings.Add(new Binding("Text", this.m_client, "Name"));
        }

        void m_client_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Layers":
                    this.pic.Image = this.m_client.Layers[0].Image;
                    if (this.pic.ClientSize.Width < this.pic.Image.Width)
                    {
                        this.Width += this.pic.Image.Width - this.pic.ClientSize.Width;
                    }
                    if (this.pic.ClientSize.Height < this.pic.Image.Height)
                    {
                        this.Height += this.pic.Image.Height - this.pic.ClientSize.Height;
                    }
                    break;
            }
        }

        private async void Form2_Shown(object sender, EventArgs e)
        {
            await this.m_client.Start("192.168.1.2", 3389, 1024, 768, 16);
        }
    }
}

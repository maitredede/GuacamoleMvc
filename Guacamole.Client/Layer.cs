using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    public abstract class Layer
    {
        private Bitmap m_bmp;
        private Graphics m_g;

        protected abstract bool AutoSize { get; }
        public Image Image { get { return this.m_bmp; } }

        internal Layer(int width, int height)
        {
            this.m_bmp = new Bitmap(width, height);
            this.m_g = Graphics.FromImage(this.m_bmp);
        }

        internal virtual void Resize(int width, int height)
        {
            this.m_g.Dispose();
            this.m_bmp.Dispose();
            this.m_bmp = new Bitmap(width, height);
            this.m_g = Graphics.FromImage(this.m_bmp);
        }

        public void Dispose()
        {
            this.m_g.Dispose();
            this.m_bmp.Dispose();
        }

        internal void DrawPng(Operation mask, Image png, int x, int y)
        {
            if (this.AutoSize)
            {
                this.Resize(png.Width, png.Height);
            }
            CompositingMode oldMode = this.m_g.CompositingMode;
            try
            {
                switch (mask)
                {
                    case Operation.Copy:
                        this.m_g.DrawImageUnscaled(png, x, y);
                        break;
                    case Operation.SourceOver:
                        this.m_g.CompositingMode = CompositingMode.SourceOver;
                        this.m_g.DrawImageUnscaled(png, x, y);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                this.m_g.CompositingMode = oldMode;
            }
        }

        internal void CopyFrom(Layer sl, Operation mask, int srcx, int srcy, int srcwidth, int srcheight, int dstx, int dsty)
        {
            sl.m_g.Flush(FlushIntention.Sync);

            if (this.AutoSize)
            {
                //this.Resize(png.Width, png.Height);
            }
            Rectangle srcRect = new Rectangle(srcx, srcy, srcwidth, srcheight);
            Rectangle destRect = new Rectangle(dstx, dsty, srcwidth, srcheight);
            CompositingMode oldMode = this.m_g.CompositingMode;
            try
            {
                switch (mask)
                {
                    case Operation.Copy:
                        this.m_g.DrawImage(sl.m_bmp, srcRect: srcRect, destRect: destRect, srcUnit: GraphicsUnit.Pixel);
                        break;
                    case Operation.SourceOver:
                        this.m_g.CompositingMode = CompositingMode.SourceOver;
                        this.m_g.DrawImage(sl.m_bmp, srcRect: srcRect, destRect: destRect, srcUnit: GraphicsUnit.Pixel);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                this.m_g.CompositingMode = oldMode;
            }
        }
    }
}

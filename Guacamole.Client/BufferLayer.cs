using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    internal sealed class BufferLayer : Layer
    {
        public BufferLayer(int width, int height)
            : base(width, height)
        {
        }

        protected override bool AutoSize { get { return true; } }
    }
}

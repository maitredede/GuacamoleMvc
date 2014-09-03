using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    internal sealed class Cursor
    {
        public Point HotSpot { get; set; }
        public Layer Layer { get; set; }

        public Rectangle Rect { get; set; }
    }
}

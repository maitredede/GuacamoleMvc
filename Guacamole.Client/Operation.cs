using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    public enum Operation
    {
        DestinationIn = 1,
        DestinationOut = 2,
        SourceIn = 4,
        SourceAtop = 6,
        SourceOut = 8,
        DestinationAtop = 9,
        Xor = 0xA,
        DestinationOver = 0xB,
        Copy = 0xC,
        SourceOver = 0xE,
        Lighter = 0xF,
    }
}

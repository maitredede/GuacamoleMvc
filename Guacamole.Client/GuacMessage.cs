using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole.Client
{
    [System.Diagnostics.DebuggerDisplay("{Command} : {Args}")]
    internal struct GuacMessage
    {
        public string Command { get; set; }
        public string[] Args { get; set; }
    }
}

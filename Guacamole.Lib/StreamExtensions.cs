using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole
{
    public static class StreamExtensions
    {
        public static async Task<int> ReadByteAsync(this Stream stream)
        {
            byte[] array = new byte[1];
            if (await stream.ReadAsync(array, 0, 1) == 0)
            {
                return -1;
            }
            return (int)array[0];
        }
    }
}

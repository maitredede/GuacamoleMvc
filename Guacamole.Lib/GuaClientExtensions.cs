using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guacamole
{
    public static class GuaClientExtensions
    {
        public static async Task WriteInstruction(this Stream output, string command, params object[] args)
        {
            await WriteInstruction(output, Encoding.UTF8, command, args);
        }

        public static async Task WriteInstruction(this Stream output, Encoding enc, string command, params object[] args)
        {
            Debug.WriteLine(string.Format("Sending '{0}' + {1} args", command, args.Length));

            //command
            await WritePart(output, enc, command);

            for (int i = 0; i < args.Length; i++)
            {
                await WriteSep(output, ',');

                object arg = args[i];
                if (arg == null)
                {
                    await WritePart(output, enc, string.Empty);
                    continue;
                }
                if (arg is byte[])
                {
                    await WritePart(output, enc, (byte[])arg);
                    continue;
                }
                await WritePart(output, enc, arg.ToString());
            }
            await WriteSep(output, ';');

            await output.FlushAsync();
        }

        private static async Task WritePart(Stream output, Encoding enc, string item)
        {
            byte[] arr = enc.GetBytes(item);
            await WritePart(output, enc, arr);
        }

        private static async Task WritePart(Stream output, Encoding enc, byte[] arr)
        {
            byte[] arrLength = enc.GetBytes(arr.Length.ToString(CultureInfo.InvariantCulture) + ".");
            await output.WriteAsync(arrLength, 0, arrLength.Length);
            await output.WriteAsync(arr, 0, arr.Length);
        }

        private static async Task WriteSep(Stream output, char separator)
        {
            byte[] arr = new byte[] { (byte)separator };
            await output.WriteAsync(arr, 0, arr.Length);
        }

        //public static void ParseCommand(this string command, out string op, out string[] args)
        //{
        //    if (string.IsNullOrEmpty(command))
        //    {
        //        throw new ArgumentNullException();
        //    }
        //    List<string> lst = new List<string>();
        //    for (int i = 0; i < command.Length; i++)
        //    {

        //    }
        //}

        public static void StartReadItem(this Stream input, byte[] readBuffer, Action<string, string[]> itemRead, Action<string> error)
        {
            Debug.WriteLine(MethodInfo.GetCurrentMethod().Name);
            try
            {
                bool loop = true;
                //Read command
                string command = ReadPartAsync(input).Result;
                //Read args
                List<string> args = new List<string>();
                do
                {
                    char sep = ReadSepAsync(input).Result;
                    switch (sep)
                    {
                        case ',':
                            args.Add(ReadPartAsync(input).Result);
                            break;
                        case ';':
                            itemRead(command, args.ToArray());
                            loop = false;
                            break;
                        default:
                            error("Invalid separator");
                            return;
                    }
                } while (loop);

                ThreadPool.QueueUserWorkItem((o) => StartReadItem(input, readBuffer, itemRead, error));
            }
            catch (Exception ex)
            {
                error(ex.GetType().Name + ": " + ex.Message);
            }
        }

        public static async Task<string> ReadPartAsync(Stream input)
        {
            List<byte> buffer = new List<byte>(1024);
            while (true)
            {
                byte b = (byte)await input.ReadByteAsync();
                if (b == '.')
                {
                    break;
                }
                else
                {
                    buffer.Add(b);
                }
            }
            int partLength = int.Parse(Encoding.UTF8.GetString(buffer.ToArray()));
            byte[] partData = new byte[partLength];
            int read = await input.ReadAsync(partData, 0, partLength);
            if (read == 0)
            {
                //End of stream
                throw new Exception("Disconnected");
            }
            if (partLength != read)
            {
                System.Diagnostics.Debugger.Break();
            }
            string part = Encoding.UTF8.GetString(partData);
            return part;
        }

        public static async Task<char> ReadSepAsync(Stream input)
        {
            return (char)(byte)await input.ReadByteAsync();
        }
    }
}

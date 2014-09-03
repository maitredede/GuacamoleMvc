using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guacamole
{
    public static class GuaInstructionsExtensions
    {
        public static async Task Select(this Stream output, string protocol)
        {
            await output.WriteInstruction("select", protocol);
        }

        public static async Task Size(this Stream output, int width, int height, int dpi)
        {
            await output.WriteInstruction("size", width, height, dpi);
        }

        /// <summary>
        /// 8.hostname,4.port,6.domain,8.username,8.password,5.width,6.height,15.initial-program,11.color-depth,13.disable-audio,15.enable-printing,7.console,13.console-audio,13.server-layout,8.security,11.ignore-cert,12.disable-auth;
        /// </summary>
        /// <param name="output"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static async Task Connect_Rdp(this Stream output, string hostname = null, int? port = null, string domain = null, string username = null, string password = null, int? width = null, int? height = null, string initial_program = null, int? color_depth = null, bool? disable_audio = null, bool? enable_printing = null, bool? console = null, bool? console_audio = null, string server_layout = null, string security = null, bool? ignore_cert = null, bool? disable_auth = null)
        {
            await output.WriteInstruction("connect", hostname, port, domain, username, password, width, height, initial_program, color_depth, disable_audio, enable_printing, console, console_audio, server_layout, security, ignore_cert, disable_auth);
        }

        /// <summary>
        /// hostname,port,username,password,font-name,font-size
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public static async Task Connect_Ssh(this Stream output, string hostname = null, int? port = null, string username = null, string password = null, string font_name = null, int? font_size = null)
        {
            await output.WriteInstruction("connect", hostname, port, username, password, font_name, font_size);
        }
    }
}

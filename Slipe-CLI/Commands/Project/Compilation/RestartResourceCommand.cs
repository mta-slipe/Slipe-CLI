using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Slipe.Commands.Project
{

    class RestartResourceCommand : ProjectCommand
    {
        
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        private static Dictionary<string, int> keys = new Dictionary<string, int>()
        {
            ["a"] = 0x41,
            ["b"] = 0x42,
            ["c"] = 0x43,
            ["d"] = 0x44,
            ["e"] = 0x45,
            ["f"] = 0x46,
            ["g"] = 0x47,
            ["h"] = 0x48,
            ["i"] = 0x49,
            ["j"] = 0x4a,
            ["k"] = 0x4b,
            ["l"] = 0x4c,
            ["m"] = 0x4d,
            ["n"] = 0x4e,
            ["o"] = 0x4f,
            ["p"] = 0x50,
            ["q"] = 0x51,
            ["r"] = 0x52,
            ["s"] = 0x53,
            ["t"] = 0x54,
            ["u"] = 0x55,
            ["v"] = 0x56,
            ["w"] = 0x57,
            ["x"] = 0x58,
            ["y"] = 0x59,
            ["z"] = 0x5a,
            [" "] = 0x20,
            ["\n"] = 0x0d,
            ["0"] = 0x30,
            ["1"] = 0x31,
            ["2"] = 0x32,
            ["3"] = 0x33,
            ["4"] = 0x34,
            ["5"] = 0x35,
            ["6"] = 0x36,
            ["7"] = 0x37,
            ["8"] = 0x38,
            ["9"] = 0x39,
            ["-"] = 0xbd,
        };


        public override string Template => "restart-resource";

        public override void Run()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new SlipeException(string.Format("{0} is only supported on windows", Template));
            }


            string directory = Directory.GetCurrentDirectory();
            string[] splits = directory.Split(Path.DirectorySeparatorChar);
            string resourceName = splits[splits.Length - 1];

            Process[] processes = Process.GetProcessesByName("MTA Server");
            if (processes.Length > 1)
            {
                throw new SlipeException("Several MTA servers are running, unable to determine which to restart resource on.");
            }
            else if (processes.Length == 0)
            {
                throw new SlipeException("No MTA servers running, unable to restart resource.");
            }

            //try
            //{
            //    processes[0].StandardInput.Write(string.Format("restart {0}", resourceName));
            //} catch (Exception)
            //{
            //    throw new SlipeException("Unable to write to process input stream.");
            //}
            
            IntPtr WindowToFind = FindWindow(null, "");

            string command = string.Format("restart {0}\n", resourceName);
            for (int i = 0; i < command.Length; i++)
            {
                if (! keys.ContainsKey(command.Substring(i, 1).ToLower()))
                {
                    throw new SlipeException("Slipe is unable to restart this resource because there is a '" + command.Substring(i, 1) + "' in the name.");
                }
                PostMessage(processes[0].MainWindowHandle, 0x0104, keys[command.Substring(i, 1).ToLower()], 0);
            }
        }

    }
}

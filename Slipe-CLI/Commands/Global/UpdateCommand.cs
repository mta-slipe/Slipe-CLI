using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Slipe.Commands.Global
{
    class UpdateCommand : Command
    {
        public override string Template => "update";

        public override CommandType CommandType => CommandType.Global;

        public override void Run()
        {
            string name = string.Format("./slipe-update-{0}", DateTime.Now.ToShortDateString().Replace("/","-"));
            string path = name + ".zip";

            new WebClient().DownloadFile("http://mta-slipe.com/slipe-cli.zip", path);

            ZipFile.ExtractToDirectory(path, name);

            string command = "";
            ProcessStartInfo processInfo;
            Process process;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = ".\\install.bat";
                processInfo = new ProcessStartInfo("cmd.exe", string.Format("/c {1}", name, command));
                processInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "/" + name;

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                // *** Redirect the output ***
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                process = Process.Start(processInfo);
                process.WaitForExit();
            } else
            {
                processInfo = new ProcessStartInfo("sudo", "chmod +x install.sh");
                processInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "/" + name;

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                // *** Redirect the output ***
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                process = Process.Start(processInfo);
                process.WaitForExit();

                processInfo = new ProcessStartInfo("sudo", "./install.sh");
                processInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "/" + name;

                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                // *** Redirect the output ***
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;

                process = Process.Start(processInfo);
                process.WaitForExit();
            }


            int exitCode = process.ExitCode;

            // clean up
            Directory.Delete(name, true);
            File.Delete(path);

            if (exitCode != 0)
            {
                throw new SlipeException("Unable to update slipe, error: " + process.StandardError.ReadToEnd());
            }
        }
    }
}

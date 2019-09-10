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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                WindowsUpdate();
            } else
            {
                LinuxUpdate();
            }
        }

        private void WindowsUpdate()
        {

            string name = string.Format("./slipe-update-{0}.exe", DateTime.Now.ToShortDateString().Replace("/", "-").Replace("\\", "-"));

            string url = options.ContainsKey("dev") ? 
                "https://development.mta-slipe.com/downloads/SlipeInstaller.exe" : 
                "https://mta-slipe.com/downloads/SlipeInstaller.exe";

            new WebClient().DownloadFile(url, name);

            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo(name);
            processInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.Arguments = options.ContainsKey("dev") ? "dev" : "";

            process = Process.Start(processInfo);
        }

        private void LinuxUpdate()
        {
            string name = string.Format("./slipe-update-{0}", DateTime.Now.ToShortDateString().Replace("/", "-"));
            string path = name + ".zip";

            string url = options.ContainsKey("dev") ? 
                "https://development.mta-slipe.com/downloads/cli-linux.zip" : 
                "https://mta-slipe.com/downloads/cli-linux.zip";

            new WebClient().DownloadFile(url, path);

            ZipFile.ExtractToDirectory(path, name);

            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("sudo");
            processInfo.WorkingDirectory = Directory.GetCurrentDirectory() + "/" + name;
            processInfo.Arguments = $"./install.sh dev";

            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

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

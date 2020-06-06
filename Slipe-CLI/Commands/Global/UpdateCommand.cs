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

            try
            {
                new WebClient().DownloadFile(url, path);

                ZipFile.ExtractToDirectory(path, name);

                foreach (var file in Directory.GetFiles($"{name}/Slipe"))
                {
                    File.Copy(file, $"/var/Slipe{file.Replace($"{name}/Slipe", "")}", true);
                }

                Directory.Delete(name, true);
                File.Delete(path);
            } catch (Exception e)
            {
                Console.WriteLine("Failed to update slipe, error: " + e.Message);

                Directory.Delete(name, true);
                File.Delete(path);

                throw;
            }
        }
    }
}

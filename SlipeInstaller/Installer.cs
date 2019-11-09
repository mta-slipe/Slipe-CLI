using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Principal;

namespace SlipeInstaller
{
    public static class Installer
    {
        static string url = "https://mta-slipe.com/downloads/cli.zip";

        static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "dev")
            {
                Console.WriteLine("Updating from dev environment");
                url = "https://development.mta-slipe.com/downloads/cli.zip";
            }

            try
            {
                Install(url);
            } catch (Exception e)
            {
                LaunchAsAdmin(string.Join(" ", args));
                Install(url);
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        public static void Install(string url)
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string installationPath = Path.Combine(programFiles, "Slipe");
            string zipPath = "cli.zip";

            AddToPath(installationPath);

            if (Directory.Exists(installationPath))
            {
                Directory.Delete(installationPath, true);
            }

            WebClient client = new WebClient();
            client.DownloadFile(url, zipPath);
            try
            {
                ZipFile.ExtractToDirectory(zipPath, installationPath);

                Console.WriteLine("Installation successfull");
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong when installing");
                Console.WriteLine(e.Message);
            }
            finally
            {
                File.Delete(zipPath);
            }
        }

        public static void AddToPath(string directory)
        {
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            if (! path.Contains(directory))
            {
                Environment.SetEnvironmentVariable("PATH", path + ";" + directory, EnvironmentVariableTarget.Machine);
            }
        }

        public static void LaunchAsAdmin(string args)
        {
            ProcessStartInfo proc = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
            {
                UseShellExecute = true,
                Verb = "runas",
                Arguments = args
            };

            try
            {
                Process.Start(proc);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Security.Principal;

namespace SlipeInstaller
{
    class Program
    {
        static string url = "https://mta-slipe.com/downloads/cli.zip";

        static void Main(string[] args)
        {
            if (args.Length >= 1 && args[0] == "dev")
            {
                Console.WriteLine("Updating from dev environment");
                url = "https://development.mta-slipe.com/downloads/cli.zip";
            }
            LaunchAsAdmin(string.Join(" ", args));

            Install();

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        static void Install()
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string installationPath = Path.Combine(programFiles, "Slipe");
            string zipPath = "cli.zip";

            if (Directory.Exists(installationPath))
            {
                Directory.Delete(installationPath, true);
            }

            WebClient client = new WebClient();
            client.DownloadFile(url, zipPath);
            try
            {
                ZipFile.ExtractToDirectory(zipPath, installationPath);

                AddToPath(installationPath);

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

        static void AddToPath(string directory)
        {
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            if (! path.Contains(directory))
            {
                Environment.SetEnvironmentVariable("PATH", path + ";" + directory, EnvironmentVariableTarget.Machine);
            }
        }

        static void LaunchAsAdmin(string args)
        {
            if (!IsRunAsAdmin())
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

        static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}

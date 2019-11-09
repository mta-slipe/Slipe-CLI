using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SlipeWpfInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShutdownButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void InstallButtonClick(object sender, RoutedEventArgs e)
        {
            AttemptInstall(this.devCheckbox.IsChecked.Value, this.dotnetCheckbox.IsChecked.Value, "3.0.100");
        }


        private void AttemptInstall(bool isDev, bool installDotNetCore, string minVersion)
        {
            Task.Run(() =>
            {
                if (installDotNetCore)
                {
                    InstallDotNetCore(minVersion);
                }

                string path = $"SlipeInstaller.exe";
                if (!File.Exists(path))
                {
                    string url = $"https://{(isDev ? "development." : "")}mta-slipe.com/downloads/SlipeInstaller.exe";
                    OutputToErrorBlock($"Downloading from {url} ...\n");
                    (new WebClient()).DownloadFile(url, path);
                    OutputToErrorBlock($"Finished downloading from {url}\n");
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo(path)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = isDev ? "dev" : "",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                try
                {
                    var process = Process.Start(processStartInfo);
                    OutputToErrorBlock($"Installation started {(isDev ? "(dev)" : "")}\n");

                    process.WaitForExit();
                    File.Delete(path);
                    if (process.ExitCode == 0)
                    {
                        OutputToErrorBlock($"Installation successful\n");
                    }
                    else
                    {
                        OutputToErrorBlock($"Installation failed, Code {process.ExitCode}\n");
                    }
                }
                catch (Exception e)
                {
                    OutputToErrorBlock($"Installation failed\n{e.Message}\n");
                }
            });
        }

        private void InstallDotNetCore(string minVersion)
        {
            string version = GetDotNetCoreVersion().Split("-")[0];

            if (version == null || new Version(version) < new Version(minVersion.Split("-")[0]))
            {
                string url = $"https://download.visualstudio.microsoft.com/download/pr/53f250a1-318f-4350-8bda-3c6e49f40e76/e8cbbd98b08edd6222125268166cfc43/dotnet-sdk-{minVersion}-win-x64.exe";
                using (WebClient client = new WebClient())
                {
                    OutputToErrorBlock($"Downloading dotnet core\n");
                    client.DownloadFile(url, $"dotnet-sdk-{minVersion}-win-x64.exe");

                    var process = Process.Start(new ProcessStartInfo($"dotnet-sdk-{minVersion}-win-x64.exe")
                    {

                    });
                    OutputToErrorBlock($"Waiting for dotnet core installer to close\n");
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        OutputToErrorBlock($"dotnet core successfully installed\n");
                    }
                    else
                    {
                        OutputToErrorBlock($"failed to install dotnet core (Exit code {process.ExitCode})\n");
                    }
                }
            } else
            {
                OutputToErrorBlock($"A sufficient dotnet core version is already installed ({version})\n");
            }
        }

        private string GetDotNetCoreVersion()
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo("dotnet")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "--version",
                RedirectStandardOutput = true
            };
            Process process = Process.Start(processStartInfo);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return process.ExitCode == 0 ? output : null;
        }

        private void OutputToErrorBlock(string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                errorBlock.Text += message;
            }));
        }
    }
}

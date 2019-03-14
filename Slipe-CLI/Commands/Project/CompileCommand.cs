using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    class CompileCommand : ProjectCommand
    {
        public override string Template => "compile";

        public override void Run()
        {
            SlipeConfig config = ConfigHelper.Read();
            PrepareBuildDirectory("./Slipe/Build");

            string[] dlls = new string[config.dlls.Count];
            for (int i = 0; i < config.dlls.Count; i++)
            {
                dlls[i] = "./Slipe/DLL/" + config.dlls[i];
            }

            foreach (string project in config.compileTargets.client)
            {
                CopySourceFiles("./Source/" + project, "./Slipe/Build/Client");
            }
            PrepareDistDirectory("./Dist/Client");
            CompileSourceFiles("./Slipe/Build/Client", "Dist/Client", dlls);

            foreach(string project in config.compileTargets.server)
            {
                CopySourceFiles("./Source/" + project, "./Slipe/Build/Server");
            }
            PrepareDistDirectory("./Dist/Server");
            CompileSourceFiles("./Slipe/Build/Server", "Dist/Server", dlls);

            new GenerateMetaCommand().Run();
        }

        private void PrepareBuildDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            } else
            {
               DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                {
                    directory.Delete(true);
                }
            }
        }

        private void CopySourceFiles(string from, string to)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] files = Directory.GetFiles(from, "", SearchOption.AllDirectories);
            foreach(string file in files)
            {
                if (! file.Contains("\\obj\\"))
                {
                    string target = file.Replace("./Source/", "");
                    string fullTarget = to + "/" + target;
                    
                    if (!Directory.Exists(Path.GetDirectoryName(fullTarget))){
                        Directory.CreateDirectory(Path.GetDirectoryName(fullTarget));
                    }

                    File.Copy(file, fullTarget, true);
                }
            }
        }

        private void PrepareDistDirectory(string path)
        {

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
                {
                    directory.Delete(true);
                }
            }
        }

        private void CompileSourceFiles(string directory, string to, string[] dlls)
        {
            string[] pathSplits = directory.Split("\\");
            string command = @"dotnet .\Slipe\Compiler\CSharp.lua.Launcher.dll -s " + directory + @" -d " + to + " -c ";
            command += " -l " + string.Join(";", dlls);
            Console.WriteLine(command);

            Process process = new Process();
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                FileName = "cmd.exe",
                Arguments = "/C " + command,
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

    }
}

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
            if (options.ContainsKey("module"))
            {
                string targetModule = options["module"];
                CompileModule(targetModule);
            } else
            {
                CompileProject();
            }

            GenerateMetaCommand generateMeta = new GenerateMetaCommand();
            generateMeta.ParseArguments(new string[0]);
            generateMeta.Run();
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
                    string target = file.Replace(from, "");
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

        private void CompileSourceFiles(string directory, string to, string[] dlls, bool isModule = false)
        {
            string[] pathSplits = directory.Split("\\");
            string command = @"dotnet .\Slipe\Compiler\CSharp.lua.Launcher.dll -s " + directory + @" -d " + to + " -c ";
            command += " -l " + string.Join(";", dlls);

            if (isModule)
            {
                command = command + " -module";
            }
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

        private void CopyDlls(string from, string to)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] files = Directory.GetFiles(from, "", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (!file.Contains("\\obj\\") && file.EndsWith(".dll"))
                {
                    string fullTarget = to + "/" + Path.GetFileName(file);

                    if (!Directory.Exists(Path.GetDirectoryName(fullTarget)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fullTarget));
                    }

                    File.Copy(file, fullTarget, true);
                }
            }
        }

        private List<string> GetModuleDlls(string moduleName)
        {
            SlipeModule moduleConfig = config.modules.Find(module => module.name == moduleName);

            string basePath = moduleConfig.path;
            string dllPath = basePath + "/DLL";

            List<string> dlls = new List<string>();

            if (!Directory.Exists(dllPath))
            {
                return dlls;
            }

            string[] files = Directory.GetFiles(dllPath, "", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                dlls.Add(file.Replace(".dll","!"));
            }
            return dlls;
        }

        private List<string> GetDlls(string excludeModule = null)
        {
            List<string> dlls = new List<string>();

            foreach(SlipeModule module in config.modules)
            {
                if (module.name != excludeModule)
                {
                    foreach(string dll in GetModuleDlls(module.name))
                    {
                        dlls.Add(dll);
                    }

                } else
                {
                    foreach(string dll in module.dlls)
                    {
                        dlls.Add(module.path + "/DLL/" + dll);
                    }
                }
            }

            return dlls;
        }

        private void CompileProject()
        {
            PrepareBuildDirectory("./Slipe/Build");

            List<string> dllList = GetDlls();
            string[] dlls = new string[dllList.Count];
            for (int i = 0; i < dllList.Count; i++)
            {
                dlls[i] = dllList[i];
            }

            if (!options.ContainsKey("server-only"))
            {
                foreach (string project in config.compileTargets.client)
                {
                    CopySourceFiles("./Source/" + project, "./Slipe/Build/Client");
                }
                PrepareDistDirectory("./Dist/Client");
                CompileSourceFiles("./Slipe/Build/Client", "Dist/Client", dlls);
            }

            if (!options.ContainsKey("client-only"))
            {
                foreach (string project in config.compileTargets.server)
                {
                    CopySourceFiles("./Source/" + project, "./Slipe/Build/Server");
                }
                PrepareDistDirectory("./Dist/Server");
                CompileSourceFiles("./Slipe/Build/Server", "Dist/Server", dlls);
            }
        }

        private void CompileModule(string moduleName)
        {
            SlipeModule moduleConfig = config.modules.Find(module => module.name == moduleName);
            if (moduleConfig.type != "internal")
            {
                throw new SlipeException("Only internal modules can be compiled");
            }

            string basePath = moduleConfig.path;
            string buildPath = basePath + "/Build";
            string clientBuildPath = buildPath + "/Client";
            string serverBuildPath = buildPath + "/Server";
            string distPath = basePath + "/Lua/Compiled";
            string clientDistPath = basePath + "/Lua/Compiled/Client";
            string serverDistPath = basePath + "/Lua/Compiled/Server";
            string dllPath = basePath + "/DLL";

            List<string> dlls = GetDlls(moduleName);

            PrepareBuildDirectory(buildPath);

            if (!options.ContainsKey("server-only"))
            {
                PrepareDistDirectory(clientDistPath);
                foreach (string project in moduleConfig.compileTargets.client)
                {
                    CopySourceFiles(basePath + "/" + project, clientBuildPath + "/" + project);
                }
                CompileSourceFiles(clientBuildPath, clientDistPath, dlls.ToArray(), true);
                foreach (string project in moduleConfig.compileTargets.client)
                {
                    CopyDlls(basePath + "/" + project, dllPath);
                }
            }


            if (!options.ContainsKey("client-only"))
            {
                PrepareDistDirectory(serverDistPath);
                foreach (string project in moduleConfig.compileTargets.server)
                {
                    CopySourceFiles(basePath + "/" + project, serverBuildPath + "/" + project);
                }
                CompileSourceFiles(serverBuildPath, serverDistPath, dlls.ToArray(), true);
                foreach (string project in moduleConfig.compileTargets.server)
                {
                    CopyDlls(basePath + "/" + project, dllPath);
                }
            }
        }

    }
}

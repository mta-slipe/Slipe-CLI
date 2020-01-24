using Slipe.Commands.Project.Compilation;
using Slipe.Commands.Project.Exports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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

            CreateProjectPathsFile();

            if (options.ContainsKey("exports"))
            {
                IndexExportsCommand exportsCommand = new IndexExportsCommand();
                exportsCommand.ParseArguments(new string[0]);
                exportsCommand.Run();
                exportsCommand.SaveConfig();
                this.config = ConfigHelper.Read();
            }

            CreateExportsFiles(config);
            CreateModuleTableFile(config);

            GenerateMetaCommand generateMeta = new GenerateMetaCommand();
            generateMeta.ParseArguments(new string[0]);
            generateMeta.Run();

            CheckValidityCommand checkValidityCommand = new CheckValidityCommand();
            checkValidityCommand.ParseArguments(new string[0]);
            checkValidityCommand.Run();
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

        private void CopySourceFiles(string from, string to, bool removeMain = false)
        {
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] files = Directory.GetFiles(from, "", SearchOption.AllDirectories);
            foreach(string file in files)
            {
                if ((! file.Contains("\\obj\\") && !file.Contains("/obj/")) || options.ContainsKey("generated") && file.EndsWith(".g.cs"))
                {
                    string target = file.Replace(from, "");
                    string fullTarget = to + "/" + target;
                    
                    if (!Directory.Exists(Path.GetDirectoryName(fullTarget))){
                        Directory.CreateDirectory(Path.GetDirectoryName(fullTarget));
                    }

                    File.Copy(file, fullTarget, true);
                    if (removeMain)
                    {
                        File.WriteAllText(fullTarget, File.ReadAllText(fullTarget).Replace("static void Main(", "static void NoMain("));
                    }
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

        private void CompileSourceFiles(string directory, string to, string[] dlls, string[] attributes, bool isModule = false)
        {
            string command = @"dotnet ./Slipe/Compiler/CSharp.lua.Launcher.dll -s " + directory + @" -d " + to + " -e -c -metadata";
            if(attributes.Length > 0)
            {
                command += " -a " + string.Join(";", attributes);
            }
            command += " -l " + string.Join(";", dlls);

            if (isModule)
            {
                command = command + " -module";
            }
            Console.WriteLine(command);

            Process process = new Process();
            ProcessStartInfo startInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
            } else
            {
                startInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Normal,
                    CreateNoWindow = false,
                    FileName = "dotnet",
                    Arguments = command.Replace("dotnet " , ""),
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
            }
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new SlipeException("The compiler failed with exit code " + process.ExitCode.ToString());
            }
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
                if (!file.Contains("\\obj\\") && !file.Contains("/obj/") && file.EndsWith(".dll"))
                {
                    Console.WriteLine(file);
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

        private List<string> GetAttributes()
        {
            List<string> attributes = new List<string>();

            foreach(SlipeModule module in config.modules)
            {
                foreach(string attribute in module.attributes)
                {
                    attributes.Add(attribute);
                }
            }

            return attributes;
        }

        private void CompileProject()
        {
            PrepareBuildDirectory("./Slipe/Build");

            string[] dlls = GetDlls().ToArray();
            string[] attributes = GetAttributes().ToArray();

            if (!options.ContainsKey("server-only"))
            {
                foreach (var project in config.compileTargets.client)
                {
                    CopySourceFiles("./Source/" + project, "./Slipe/Build/Client", project.BlockEntryPoint == true ? true : false);
                }
                PrepareDistDirectory("./Dist/Client");
                CompileSourceFiles("./Slipe/Build/Client", "Dist/Client", dlls, attributes);
            }

            if (!options.ContainsKey("client-only"))
            {
                foreach (var project in config.compileTargets.server)
                {
                    CopySourceFiles("./Source/" + project, "./Slipe/Build/Server", project.BlockEntryPoint == true ? true : false);
                }
                PrepareDistDirectory("./Dist/Server");
                CompileSourceFiles("./Slipe/Build/Server", "Dist/Server", dlls, attributes);
            }
        }

        private void CompileModule(string moduleName)
        {
            SlipeModule moduleConfig = config.modules.Find(module => module.name == moduleName);
            if (moduleConfig.type != "internal")
            {
                throw new SlipeException("Only internal modules can be compiled.");
            }

            string basePath = moduleConfig.path;
            string buildPath = basePath + "/Build";
            string clientBuildPath = buildPath + "/Client";
            string serverBuildPath = buildPath + "/Server";
            string distPath = basePath + "/Lua/Compiled";
            string clientDistPath = basePath + "/Lua/Compiled/Client";
            string serverDistPath = basePath + "/Lua/Compiled/Server";
            string dllPath = basePath + "/DLL";

            string[] dlls = GetDlls(moduleName).ToArray();
            string[] attributes = GetAttributes().ToArray();

            PrepareBuildDirectory(buildPath);

            if (!options.ContainsKey("server-only"))
            {
                PrepareDistDirectory(clientDistPath);
                foreach (var project in moduleConfig.compileTargets.client)
                {
                    CopyDlls(basePath + "/" + project, dllPath);
                    CopySourceFiles(basePath + "/" + project, clientBuildPath + "/" + project, project.BlockEntryPoint == true ? true : false);
                }
                CompileSourceFiles(clientBuildPath, clientDistPath, dlls, attributes, true);
            }


            if (!options.ContainsKey("client-only"))
            {
                PrepareDistDirectory(serverDistPath);
                foreach (var project in moduleConfig.compileTargets.server)
                {
                    CopyDlls(basePath + "/" + project, dllPath);
                    CopySourceFiles(basePath + "/" + project, serverBuildPath + "/" + project, project.BlockEntryPoint == true ? true : false);
                }
                CompileSourceFiles(serverBuildPath, serverDistPath, dlls, attributes, true);

            }
        }

        private void CreateProjectPathsFile()
        {
            string projectPaths = "projectPaths = {\n";

            foreach(var project in config.compileTargets.server)
            {
                string[] splits = ((string)project).Split("/");
                projectPaths += string.Format("\t['{0}'] = '{1}',\n", splits[splits.Length - 1], "Source/" + project);
            }
            foreach(var project in config.compileTargets.client)
            {
                string[] splits = ((string)project).Split("/");
                projectPaths += string.Format("\t['{0}'] = '{1}',\n", splits[splits.Length - 1], "Source/" + project);
            }

            foreach(var module in config.modules)
            {
                if (module.compileTargets == null)
                {
                    module.compileTargets = new SlipeConfigCompileTargetList();
                }
                foreach (var project in module.compileTargets.server)
                {
                    string[] splits = ((string)project).Split("/");
                    projectPaths += string.Format("\t['{0}'] = '{1}',\n", splits[splits.Length - 1], module.path + "/" + project);
                }
                foreach (var project in module.compileTargets.client)
                {
                    string[] splits = ((string)project).Split("/");
                    projectPaths += string.Format("\t['{0}'] = '{1}',\n", splits[splits.Length - 1], module.path + "/" + project);
                }
            }

            projectPaths += "}";
            Directory.CreateDirectory("Dist");
            File.WriteAllText("Dist/projectPaths.lua", projectPaths);
        }

        private void CreateExportsFiles(SlipeConfig config)
        {
            string clientExports = "";
            foreach (SlipeExport export in config.exports)
            {
                if (export.type == "client")
                {
                    clientExports += string.Format("function {0}(...)\n\t{1}(...)\nend\n", export.niceName, export.name);
                }
            }
            clientExports += "";
            File.WriteAllText("Dist/Client/Exports.lua", clientExports);

            string serverExports = "";
            foreach (SlipeExport export in config.exports)
            {
                if (export.type == "server")
                {
                    serverExports += string.Format("function {0}(...)\n\t{1}(...)\nend\n", export.niceName, export.name);
                }
            }
            serverExports += "";
            File.WriteAllText("Dist/Server/Exports.lua", serverExports);
        }

        private void CreateModuleTableFile(SlipeConfig config)
        {
            string moduleTable = "moduleTable = {";
            foreach (SlipeModule module in config.modules)
            {
                moduleTable += string.Format("\"{0}\", ", module.path);
            }
            moduleTable += "}";
            File.WriteAllText("Dist/Client/Modules.lua", moduleTable);
            File.WriteAllText("Dist/Server/Modules.lua", moduleTable);
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Slipe.Commands.Project
{

    class BuildCommand : ProjectCommand
    {
        public override string Template => "build";
        private string outputDirectory;

        public override void Run()
        {
            outputDirectory = options.ContainsKey("directory") ? options["directory"] : "./BuildOutput";
            Compile();

            CopyToOutput();
            if (options.ContainsKey("luac"))
            {
                CompileToLuac().Wait();
            }
        }

        private void CopyToOutput()
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, true);
            }

            CopyFiles("./Dist/", outputDirectory + "/Dist");
            CopyFiles("./Slipe/Lua", outputDirectory + "/Slipe/Lua");

            foreach (SlipeAssetDirectory directory in config.assetDirectories)
            {
                CopyFiles("./" + directory.path, outputDirectory + "/" + directory.path);
            }

            foreach (SlipeHttpDirectory directory in config.httpDirectories)
            {
                CopyFiles("./" + directory.path, outputDirectory + "/" + directory.path);
            }

            foreach (SlipeModule module in config.modules)
            {
                CopyFiles(module.path + "/Lua", outputDirectory + "/" + module.path + "/Lua");

                foreach(SlipeAssetDirectory directory in module.assetDirectories)
                {
                    CopyFiles(module.path + "/" + directory.path, outputDirectory + "/" + module.path + "/" + directory.path);
                }

                foreach (SlipeHttpDirectory directory in module.httpDirectories)
                {
                    CopyFiles(module.path + "/" + directory.path, outputDirectory + "/" + module.path + "/" + directory.path);
                }
            }
            File.Copy("./meta.xml", outputDirectory + "/meta.xml");
        }

        private void CopyFiles(string from, string to)
        {
            if (!Directory.Exists(from))
            {
                return;
            }
            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            string[] files = Directory.GetFiles(from, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string target = file.Replace(from, "");
                string fullTarget = to + "/" + target;

                if (!Directory.Exists(Path.GetDirectoryName(fullTarget)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullTarget));
                }

                File.Copy(file, fullTarget, true);
            }
        }

        private async Task CompileToLuac()
        {
            await CompileDirectoryToLuac(outputDirectory + "/Dist");
            foreach (SlipeModule module in config.modules)
            {
                await CompileDirectoryToLuac(outputDirectory + "/" + module.path + "/Lua");
            }
        }

        private async Task CompileDirectoryToLuac(string directory)
        {
            using HttpClient httpClient = new HttpClient(new HttpClientHandler()
            {
                MaxConnectionsPerServer = 16
            });

            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            var tasks = files.Select(async file =>
            {
                byte[] content = File.ReadAllBytes(file);

                if (content.Length > 0)
                {
                    string url = "http://luac.mtasa.com?compile=1&debug=0&obfuscate=3";
                    var result = await httpClient.PostAsync(url, new ByteArrayContent(content));
                    var compiledLua = await result.Content.ReadAsByteArrayAsync();
                    File.Delete(file);
                    File.WriteAllBytes(file, compiledLua);
                }
            });
            await Task.WhenAll(tasks);
        }

        private void Compile()
        {
            foreach (SlipeModule module in config.modules)
            {
                if (module.type == "internal")
                {
                    CompileModule(module.name);
                }
            }

            CompileCommand compile = new CompileCommand();
            compile.ParseArguments(new string[0]);
            compile.Run();
        }

        private void CompileModule(string name)
        {
            CompileCommand command = new CompileCommand();
            command.ParseArguments(new string[]
            {
                "-module",
                name
            });
            command.Run();
        }

    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class CreateModuleCommand : ProjectCommand
    {
        public override string Template => "create-module";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n" + Template + " {name} [-directory {directory}]");
            }

            string name = parameters[0];
            string directory = "Modules/" + name;

            if (options.ContainsKey("directory"))
            {
                directory = options["directory"];
            }

            if (Directory.Exists(directory))
            {
                throw new SlipeException("The directory \"" + directory + "\" already exists.");
            }

            Directory.CreateDirectory(directory);

            SlipeModule module = new SlipeModule()
            {
                type = "internal",
                compileTargets = new SlipeConfigCompileTarget()
                {
                    client = new List<string>(),
                    server = new List<string>()
                },
                name = name,
                path = directory,
                assetDirectories = new List<SlipeAssetDirectory>(),
                systemComponents = new List<string>(),
                backingLua = new List<string>(),
                dlls = new List<string>()
            };
            Console.WriteLine(JsonConvert.SerializeObject(module));
            config.modules.Add(module);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class ExportModuleCommand : ProjectCommand
    {
        public override string Template => "export-module";

        public int Alpha
        {
            get;
            private set;
        }

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the module name, syntax: \n" + Template + " {name} [-directory {directory}]");
            }

            string name = parameters[0];
            string directory = "ExportedModules/" + name;

            if (options.ContainsKey("directory"))
            {
                directory = options["directory"];
            }

            SlipeModule module = new SlipeModule();
            bool found = false;
            foreach(SlipeModule slipeModule in config.modules)
            {
                if (slipeModule.name == name)
                {
                    module = slipeModule;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new SlipeException("No module by the name of '" + name + "' can be found in the configs.");
            }

            CopyFiles(module.path + "/DLL", directory + "/DLL");
            CopyFiles(module.path + "/Lua", directory + "/Lua");

            SlipeModule exportModule = new SlipeModule()
            {
                assetDirectories = module.assetDirectories,
                systemComponents = module.systemComponents,
                backingLua = module.backingLua,
                name = module.name,
                dlls = module.dlls
            };

            ConfigHelper.WriteModule(exportModule, directory + "/.slipe");

            if (options.ContainsKey("zip"))
            {
                ZipFile.CreateFromDirectory(directory, directory + ".zip");
                Directory.Delete(directory, true);
            }
        }

        private void CopyFiles(string from, string to)
        {
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
    }
}

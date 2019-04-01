using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project.Modules
{
    class DeleteModuleCommand : ProjectCommand
    {
        public override string Template => "delete-module";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n" + Template + " {name}");
            }


            if (!options.ContainsKey("y"))
            {
                Console.WriteLine("Are you sure you wish to proceed? This action can not be undone. y/n");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    return;
                }
            }

            string name = parameters[0];

            SlipeModule module = config.modules.Find((module) => module.name == name);


            DirectoryInfo directoryInfo = new DirectoryInfo(module.path);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                directory.Delete(true);
            }
            Directory.Delete(module.path);

            config.modules.Remove(module);
        }
    }
}
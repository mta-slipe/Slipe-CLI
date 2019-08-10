using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project.Project
{
    class DeleteProjectCommand : ProjectCommand
    {
        public override string Template => "delete-project";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \nslipe delete-project {project-name} [-server] [-client]");
            }

            if (!options.ContainsKey("y"))
            {
                Console.WriteLine("Are you sure you wish to proceed? This action can not be undone. y/n");
                if (Console.ReadKey().Key != ConsoleKey.Y)
                {
                    return;
                }
            }
            SlnFile solution = new SlnFile($"{Path.GetFileName(Directory.GetCurrentDirectory())}.sln");

            string name = parameters[0];
            string path = "Source/" + name;

            if (targetsModule)
            {
                path = targetModule.path + "/" + path;
            }

            // delete everything
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                directory.Delete(true);
            }
            Directory.Delete(path);
            
            SlipeConfigCompileTarget target = options.ContainsKey("module") ? targetModule.compileTargets : config.compileTargets;
            target.server.Remove(name);
            target.client.Remove(name);

            string csProjPath = path + "/" + name + ".csproj";
            solution.RemoveProject(name, csProjPath);

        }
    }
}

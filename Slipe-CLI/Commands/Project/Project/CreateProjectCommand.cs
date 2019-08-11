using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Slipe.Commands.Project.Project
{
    class CreateProjectCommand : ProjectCommand
    {

        public override string Template => "create-project";

        public override void Run()
        {

            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \nslipe create-project {project-name} [-server] [-client]");
            }
            string name = parameters[0];
            string path = "Source/" + name;

            if (targetsModule)
            {
                path = targetModule.path + "/" + path;
            }
            SlnFile solution = new SlnFile($"{Path.GetFileName(Directory.GetCurrentDirectory())}.sln");


            string csProjPath = path + "/" + name + ".csproj";

            if (Directory.Exists(path))
            {
                throw new SlipeException("A directory with this name already exists.");
            }

            Directory.CreateDirectory(path);

            CsprojFile projectFile = new CsprojFile(csProjPath, name, "netcoreapp3.0");
            projectFile.Save();

            solution.AddProject(name, csProjPath);

            SlipeConfigCompileTargetList target = targetsModule ? targetModule.compileTargets : config.compileTargets;
            if (options.ContainsKey("server"))
            {
                target.server.Add(name);
            }
            if (options.ContainsKey("client"))
            {
                target.client.Add(name);
            }
        }
    }
}
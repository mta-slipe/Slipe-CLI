using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    class AddProjectCommand : ProjectCommand
    {
        public override string Template => "add-project";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \nslipe add-project {project-name} [-server] [-client]");
            }

            string projectName = parameters[0];

            if (! options.ContainsKey("server") && ! options.ContainsKey("client"))
            {
                throw new SlipeException("Please specify server or client (or both): \nslipe add-project {project-name} [-server] [-client]");
            }
            SlipeConfig config = ConfigHelper.Read();

            if (options.ContainsKey("server"))
            {
                if (! Directory.Exists("./Source/Server/" + projectName) && !options.ContainsKey("force"))
                {
                    throw new SlipeException("No project by this name is found in ./Source/Server, if you wish to add it anyway use -force");
                }
                config.compileTargets.server.Add(projectName);
            }
            if (options.ContainsKey("client"))
            {
                if (!Directory.Exists("./Source/Client/" + projectName) && !options.ContainsKey("force"))
                {
                    throw new SlipeException("No project by this name is found in ./Source/Server, if you wish to add it anyway use -force");
                }
                config.compileTargets.client.Add(projectName);
            }

            ConfigHelper.Write(config);
        }
    }
}

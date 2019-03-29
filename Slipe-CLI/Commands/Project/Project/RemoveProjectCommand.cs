using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe.Commands.Project.Project
{
    class RemoveProjectCommand : ProjectCommand
    {
        public override string Template => "remove-project";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \nslipe remove-project {project-name} [-server] [-client]");
            }

            string projectName = parameters[0];

            if (! options.ContainsKey("server") && ! options.ContainsKey("client"))
            {
                throw new SlipeException("Please specify server or client (or both): \nslipe remove-project {project-name} [-server] [-client]");
            }
            SlipeConfig config = ConfigHelper.Read();

            if (options.ContainsKey("server"))
            {
                if (!config.compileTargets.server.Contains(projectName))
                {
                    throw new SlipeException(projectName + " not found in server");
                }
                config.compileTargets.server.Remove(projectName);
            }
            if (options.ContainsKey("client"))
            {
                if (!config.compileTargets.server.Contains(projectName))
                {
                    throw new SlipeException(projectName + " not found in client");
                }
                config.compileTargets.client.Remove(projectName);
            }

            ConfigHelper.Write(config);
        }
    }
}

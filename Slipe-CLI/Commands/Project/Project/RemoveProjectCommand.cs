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
            string path = "Source/" + projectName;

            if (! options.ContainsKey("server") && ! options.ContainsKey("client"))
            {
                throw new SlipeException("Please specify server or client (or both): \nslipe remove-project {project-name} [-server] [-client]");
            }


            SlipeConfigCompileTargetList target = targetsModule ? targetModule.compileTargets : config.compileTargets;

            if (options.ContainsKey("server"))
            {
                if (!target.server.Contains(path))
                {
                    throw new SlipeException(projectName + " not found in server");
                }
                target.server.Remove(path);
            }
            if (options.ContainsKey("client"))
            {
                if (!target.server.Contains(path))
                {
                    throw new SlipeException(projectName + " not found in client");
                }
                target.client.Remove(path);
            }
        }
    }
}

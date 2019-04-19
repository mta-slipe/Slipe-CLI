using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    abstract class ProjectCommand : Command
    {
        public override CommandType CommandType => CommandType.Project;
        protected SlipeModule targetModule;
        protected bool targetsModule;
        protected SlipeConfig config;
        

        public override void ParseArguments(string[] args)
        {
            config = ConfigHelper.Read();
            base.ParseArguments(args);
            if (options.ContainsKey("module"))
            {
                SlipeConfig config = ConfigHelper.Read();
                foreach (SlipeModule module in config.modules)
                {
                    if (module.name == options["module"])
                    {
                        targetModule = module;
                        targetsModule = true;
                        break;
                    }
                }
            }
        }

        public void SaveConfig()
        {
            if (targetsModule)
            {
                config.modules.RemoveAll((module) => module.name == targetModule.name);
                config.modules.Add(targetModule);
            }
            ConfigHelper.Write(config);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe.Commands.Project.Assets
{
    class AddAssetDirectoryCommand : ProjectCommand
    {
        public override string Template => "add-assets";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the project name, syntax: \n"  + Template + " {directory-path} [-no-download]");
            }

            string assetDirectory = parameters[0];

            SlipeAssetDirectory directory = new SlipeAssetDirectory
            {
                path = assetDirectory,
                downloads = ! options.ContainsKey("no-download")
            };

            if (targetsModule)
            {
                targetModule.assetDirectories.Add(directory);
            } else
            {
                config.assetDirectories.Add(directory);
            }
        }
    }
}

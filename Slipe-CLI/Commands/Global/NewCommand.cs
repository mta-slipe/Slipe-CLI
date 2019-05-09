using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Slipe.Commands.Global
{
    class NewCommand : GlobalCommand
    {
        const string coreUrl = "http://mta-slipe.com/downloads/core.zip";

        public override string Template => "new";
        public override CommandType CommandType => CommandType.NonProject;

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify a project name, syntax:\nslipe new {project-name}");
            }
            string name = parameters[0];
            string directory = "./" + name;

            if (Directory.Exists(directory) && !options.ContainsKey("force"))
            {
                throw new SlipeException("A directory with that name already exists, if you wish to continue anyway use -force");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            new WebClient().DownloadFile(coreUrl, directory + ".zip");

            ZipFile.ExtractToDirectory(directory + ".zip", directory);
            File.Delete(directory + ".zip");
        }
    }
}

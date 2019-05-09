using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Slipe.Commands.Project
{
    class UpdateCoreCommand : ProjectCommand
    {
        const string coreUrl = "http://mta-slipe.com/downloads/core.zip";

        public override string Template => "update-core";

        public override void Run()
        {
            UpdateCoreModule();
        }

        private void UpdateCoreModule()
        {
            string name = string.Format("./slipe-{0}", DateTime.Now.ToShortDateString());
            string path = name + ".zip";

            new WebClient().DownloadFile(coreUrl, path);

            ZipFile.ExtractToDirectory(path, name);

            // copy Slipe/Core from zip to project
            string sourcePath = name + "/Slipe/Core";
            string destinationPath = "./Slipe/Core";
            CopyFiles(sourcePath, destinationPath);
            CopyFiles(name + "/Slipe/Compiler", "./Slipe/Compiler");

            SlipeConfig newConfig = ConfigHelper.Read(name + "/.slipe");
            SlipeModule newModuleConfig = new SlipeModule();

            // locate core module config
            for (int i = 0; i < newConfig.modules.Count; i++)
            {
                var module = newConfig.modules[i];
                if (module.name == "SlipeCore")
                {
                    newModuleConfig = module;
                    break;
                }
            }

            for (int i = 0; i < config.modules.Count; i++)
            {
                var module = config.modules[i];
                if (module.name == "SlipeCore")
                {
                    config.modules[i] = newModuleConfig;
                    break;
                }
            }

            // clean up
            Directory.Delete(name, true);
            File.Delete(path);
        }

        private void CopyFiles(string sourcePath, string destinationPath)
        {
            if (Directory.Exists(destinationPath))
            {
                Directory.Delete(destinationPath, true);
            }

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
            }

            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
            }
        }
    }
}

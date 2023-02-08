using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Slipe.Commands.Project
{
    class GenerateMetaCommand : ProjectCommand
    {
        public override string Template => "meta-generate";

        private XmlDocument meta;
        private XmlElement root;

        public override void Run()
        {
            meta = new XmlDocument();
            root = meta.CreateElement("meta");

            CreateSystemElements(config);
            // CreateMTAElements();


            foreach(SlipeModule module in config.modules)
            {
                IndexDirectory(module.path + "/Lua/Compiled/Server", "server");
                IndexDirectory(module.path + "/Lua/Compiled/Client", "client");
            }

            IndexDirectory("./Dist/Server", "server");
            IndexDirectory("./Dist/Client", "client");

            CreateMainElements();
            CreateFileElements(config);
            CreateHttpElements(config);
            CreateMinVersion();

            CreateExportElements(config);


            meta.AppendChild(root);
            meta.Save("./meta.xml");

        }

        private void CreateSystemElements(SlipeConfig config)
        {
            foreach (string systemComponent in config.systemComponents)
            {
                XmlElement element = meta.CreateElement("script");
                element.SetAttribute("src", "Slipe/Lua/System/" + systemComponent);
                element.SetAttribute("type", "shared");
                root.AppendChild(element);
            }
            foreach(SlipeModule module in config.modules)
            {
                if (module.systemComponents != null)
                {
                    foreach(string systemComponent in module.systemComponents)
                    {
                        XmlElement element = meta.CreateElement("script");
                        element.SetAttribute("src", module.path + "/Lua/SystemComponents/" + systemComponent);
                        element.SetAttribute("type", "shared");
                        root.AppendChild(element);
                    }
                }
                if (module.backingLua != null)
                {
                    foreach(string backingFile in module.backingLua)
                    {
                        XmlElement element = meta.CreateElement("script");
                        element.SetAttribute("src", module.path + "/Lua/Backing/" + backingFile);
                        element.SetAttribute("type", "shared");
                        root.AppendChild(element);
                    }
                }
            }
        }

        private void IndexDirectory(string directory, string scriptType)
        {
            Console.WriteLine("Indexing {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            foreach(string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Replace("\\", "/");
                if (relativePath.StartsWith("."))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (! file.EndsWith("manifest.lua"))
                {
                    XmlElement element = meta.CreateElement("script");
                    element.SetAttribute("src", relativePath);
                    element.SetAttribute("type", scriptType);
                    root.AppendChild(element);
                } else
                {
                    XmlElement element = meta.CreateElement("file");
                    element.SetAttribute("src", relativePath);
                    root.AppendChild(element);
                }
            }
        }

        private void CreateMainElements()
        {
            XmlElement projectPathsElement = meta.CreateElement("script");
            projectPathsElement.SetAttribute("src", "Dist/projectPaths.lua");
            projectPathsElement.SetAttribute("type", "shared");
            root.AppendChild(projectPathsElement);

            XmlElement element = meta.CreateElement("script");
            element.SetAttribute("src", "Slipe/Lua/Main/main.lua");
            element.SetAttribute("type", "shared");
            root.AppendChild(element);
        }

        private void CreateMinVersion()
        {
            XmlElement element = meta.CreateElement("min_mta_version");
            element.SetAttribute("server", config.serverMinVersion);
            element.SetAttribute("client", config.clientMinVersion);
            root.AppendChild(element);
        }

        private void CreateFileElements(SlipeConfig config)
        {
            foreach(SlipeAssetDirectory directory in config.assetDirectories)
            {
                IndexDirectoryForFiles(directory.path, directory.downloads, directory.extension);
            }
            foreach(SlipeModule module in config.modules)
            {
                foreach (SlipeAssetDirectory directory in module.assetDirectories)
                {
                    IndexDirectoryForFiles(module.path + "/" + directory.path, directory.downloads, directory.extension);
                }
            }
        }

        private void CreateHttpElements(SlipeConfig config)
        {
            foreach (SlipeHttpDirectory directory in config.httpDirectories)
            {
                IndexDirectoryForHttpFiles(directory.path, directory.interpretedFiles, config.defaultHttpFile);
            }
            foreach (SlipeModule module in config.modules)
            {
                foreach (SlipeHttpDirectory directory in module.httpDirectories)
                {
                    IndexDirectoryForHttpFiles(module.path + "/" + directory.path, directory.interpretedFiles, config.defaultHttpFile);
                }
            }
        }

        private void CreateExportElements(SlipeConfig config)
        {
            foreach(SlipeExport export in config.exports)
            {
                XmlElement element = meta.CreateElement("export");
                element.SetAttribute("function", export.niceName);
                element.SetAttribute("type", export.type);
                if (export.isHttp)
                {
                    element.SetAttribute("http", "true");
                }
                root.AppendChild(element);
            }
        }

        private void IndexDirectoryForFiles(string directory, bool downloads = true, string extension = null)
        {
            Console.WriteLine("Indexing {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (extension == null || file.EndsWith(extension))
                {
                    string relativePath = file.Replace("\\", "/");
                    if (relativePath.StartsWith("."))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    if (relativePath.StartsWith("/"))
                    {
                        relativePath = relativePath.Substring(1);
                    }
                    XmlElement element = meta.CreateElement("file");
                    element.SetAttribute("src", relativePath);
                    element.SetAttribute("download", downloads.ToString().ToLower());
                    root.AppendChild(element);
                }
            }
        }

        private void IndexDirectoryForHttpFiles(string directory, List<string> interpretedFiles, string defaultFile)
        {
            Console.WriteLine("Indexing {0}", directory);
            if (!Directory.Exists(directory))
            {
                return;
            }
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                string relativePath = file.Replace("\\", "/");
                if (relativePath.StartsWith("."))
                {
                    relativePath = relativePath.Substring(1);
                }
                if (relativePath.StartsWith("/"))
                {
                    relativePath = relativePath.Substring(1);
                }
                XmlElement element = meta.CreateElement("html");
                element.SetAttribute("src", relativePath);
                element.SetAttribute("raw", "true");
                if (relativePath == defaultFile)
                {
                    element.SetAttribute("default", "true");
                }

                if(interpretedFiles.Any((path) =>
                {
                    return relativePath.EndsWith(path);
                }))
                {
                    element.SetAttribute("raw", "false");
                }
                root.AppendChild(element);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Slipe.Commands.Project.Exports
{
    class IndexExportsCommand : ProjectCommand
    {
        public override string Template => "exports";
        private Type ExportAttributeType;

        public override void Run()
        {
            config.exports = new List<SlipeConfigExport>();

            IndexDirectory("./Slipe/Build/Server", "server");
            IndexDirectory("./Slipe/Build/Client", "client");

            foreach(SlipeModule module in config.modules)
            {
                IndexDirectory(module.path + "/Build/Server", "server");
                IndexDirectory(module.path + "/Build/Client", "client");
            }
        }

        public void IndexDirectory(string path, string type)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("No dir {0}", path);
                return;
            }

            List<Assembly> assemblies = new List<Assembly>();
            foreach (string file in Directory.GetFiles(path, "*.dll", new EnumerationOptions()
            {
                RecurseSubdirectories = true
            }))
            {
                Assembly assembly = LoadAssembly(file);
                if (assembly != null)
                {
                    assemblies.Add(assembly);
                }
            }

            if (ExportAttributeType == null)
            {
                throw new SlipeException("ExportAttribute not found");
            }

            foreach (Assembly assembly in assemblies)
            {
                string[] exports = GetAssemblyExports(assembly);
                foreach (string export in exports)
                {
                    config.exports.Add(new SlipeConfigExport()
                    {
                        name = export,
                        type = type
                    });
                }
            }
        }

        public Assembly LoadAssembly(string path)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path.Replace("./", "").Replace("/", "\\"));
            try
            {
                Assembly assembly = Assembly.LoadFrom(fullPath);


                foreach (Type type in assembly.GetTypes())
                {
                    if (type.FullName == "Slipe.Shared.Exports.ExportAttribute")
                    {
                        ExportAttributeType = type;
                        break;
                    }

                }
                return assembly;
            } catch (Exception e)
            {
                if (e.Message != "Assembly with same name is already loaded")
                {
                    throw;
                }
            }
            return null;
        }

        public string[] GetAssemblyExports(Assembly assembly)
        {
            var types = assembly.GetTypes();

            var methods = types
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(ExportAttributeType, false).Length > 0)
                .ToArray();

            string[] exports = new string[methods.Length];
            for (int i = 0; i < methods.Length; i++)
            {
                MethodInfo method = methods[i];
                exports[i] = method.DeclaringType.FullName + "." + method.Name;
                if (! method.IsStatic)
                {
                    throw new SlipeException("Only static methods can be exported.");
                }
            }
            return exports;
        }
    }
}

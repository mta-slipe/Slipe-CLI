using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Slipe.Commands.Project.Compilation
{
    class CheckValidityCommand : ProjectCommand
    {
        public override string Template => "check-validity";

        public override void Run()
        {
            config.exports = new List<SlipeExport>();

            foreach (SlipeModule module in config.modules)
            {
                if (module.type == "internal")
                {
                    IndexDirectory(module.path + "/Build/Server", "server");
                    IndexDirectory(module.path + "/Build/Client", "client");
                }
            }

            IndexDirectory("./Slipe/Build/Server", "server");
            IndexDirectory("./Slipe/Build/Client", "client");
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
                try
                {
                    Assembly assembly = LoadAssembly(file);
                    if (assembly != null)
                    {
                        assemblies.Add(assembly);
                    }
                } catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach (Assembly assembly in assemblies)
            {
                AssessAssemblyValidity(assembly);
            }
        }

        public Assembly LoadAssembly(string path)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path.Replace("./", "").Replace("/", "\\"));
            try
            {
                Assembly assembly = Assembly.LoadFrom(fullPath);

                return assembly;
            }
            catch (Exception e)
            {
                if (e.Message != "Assembly with same name is already loaded" && !e.Message.Contains("Format of the executable"))
                {
                    throw;
                }
            }
            return null;
        }

        public void AssessAssemblyValidity(Assembly assembly)
        {
            var types = assembly.GetTypes();

            var rpcs = types
                .Where(m => m.GetInterfaces().Any((i) => i.FullName == "Slipe.Shared.Rpc.IRpc"))
                .Where(m => !m.IsAbstract && !m.IsInterface)
                .ToArray();

            foreach (var rpc in rpcs)
            {
                if (rpc.GetConstructor(new Type[] { }) == null)
                {
                    string error = $"RPC class {rpc.FullName} does not implement a default (parameterless) constructor.";
                    Console.WriteLine($"Slipe: error RPC-INVALID: {error}");
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    class AddDllCommand : ProjectCommand
    {
        public override string Template => "add-dll";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the dll name, syntax: \nslipe add-dll {dll-name}");
            }

            string dllName = parameters[0];

            if (!File.Exists("./Slipe/DLL/" + dllName) && !options.ContainsKey("force"))
            {
                throw new SlipeException("No dll by this name is found in ./Slipe/DLL, if you wish to add it anyway use -force");
            }
            
            config.dlls.Add(dllName);
        }
    }
}

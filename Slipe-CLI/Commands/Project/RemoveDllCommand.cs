using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Slipe.Commands.Project
{
    class RemoveDllCommand : ProjectCommand
    {
        public override string Template => "remove-dll";

        public override void Run()
        {
            if (parameters.Count < 1)
            {
                throw new SlipeException("Please specify the dll name, syntax: \nslipe remove-dll {dll-name}");
            }

            string dllName = parameters[0];

            if (!config.dlls.Contains(dllName))
            {
                throw new SlipeException(dllName + " not found in dll list");
            }


            config.dlls.Remove(dllName);
        }
    }
}

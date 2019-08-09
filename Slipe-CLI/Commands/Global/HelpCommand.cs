using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Slipe.Commands.Global
{
    class HelpCommand : GlobalCommand
    {
        public override string Template => "help";
        public override CommandType CommandType => CommandType.Global;

        public override void Run()
        {
            foreach (Type type in
                Assembly.GetAssembly(typeof(Command)).GetTypes()
                .Where(
                    myType => myType.IsClass &&
                    !myType.IsAbstract &&
                    myType.IsSubclassOf(typeof(Command)
                )))
            {
                Command command = (Command)Activator.CreateInstance(type);
                Console.WriteLine($"slipe {command.Template}");
            }
            Console.WriteLine("\nVisit https://mta-slipe.com/docs/cli.html for documentation for all commands");

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe.Commands
{
    abstract class Command
    {
        protected Dictionary<string, string> options;
        protected List<string> parameters;

        public abstract string Template { get; }
        public abstract bool IsProjectCommand { get; }
        
        public abstract void Run();

        public virtual void ParseArguments(string[] args)
        {
            string previous = null;
            foreach(string argument in args)
            {
                if (argument.StartsWith("-"))
                {
                    previous = argument;
                } else
                {
                    if (previous != null)
                    {
                        options[previous.Substring(1)] = argument;
                    } else
                    {
                        parameters.Add(argument);
                    }
                    previous = null;
                }
            }
        }

        public bool Matches(string[] args)
        {
            return args.Length > 0 && args[0] == Template;
        }
    }
}

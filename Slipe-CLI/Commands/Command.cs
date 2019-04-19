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
        public abstract CommandType CommandType { get; }
        
        public abstract void Run();

        public virtual void ParseArguments(string[] args)
        {
            parameters = new List<string>();
            options = new Dictionary<string, string>();

            string previous = null;
            foreach(string argument in args)
            {
                if (argument.StartsWith("-"))
                {
                    if (previous != null)
                    {
                        options[previous.Substring(1)] = "";
                    }
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
            if (previous != null)
            {
                options[previous.Substring(1)] = "";
            }
        }

        public bool Matches(string[] args)
        {
            return args.Length > 0 && args[0] == Template;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe.Commands.Global
{
    abstract class GlobalCommand: Command
    {
        public override CommandType CommandType => CommandType.Global;
    }
}

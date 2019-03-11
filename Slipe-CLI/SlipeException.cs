using System;
using System.Collections.Generic;
using System.Text;

namespace Slipe
{
    class SlipeException : Exception
    {
        public SlipeException(string message) : base(message)
        {
        }
    }
}

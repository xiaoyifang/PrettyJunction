using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PrettyJunction
{
    public class JunctionException:IOException
    {
        public JunctionException(string message):base(message)
        {
            
        }
        public JunctionException(string message,Exception innerException):base(message,innerException)
        {
            
        }
    }
}

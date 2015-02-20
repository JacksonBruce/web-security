using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StyleSheetsParser
{
    public class CssException:Exception
    {
        public CssException() { }
        public CssException(string message) : base(message) { }
        public CssException(string message, Exception innerException) : base(message, innerException) { }
        public CssException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

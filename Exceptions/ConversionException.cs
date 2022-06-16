using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nrlmsise.Exceptions
{
    internal class ConversionException : Exception
    {
        public TextBox callingElement;
        public ConversionException(TextBox callingElement)
        {
;            this.callingElement = callingElement;
        }
    }
}

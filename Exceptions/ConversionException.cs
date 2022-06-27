using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nrlmsise.Exceptions
{
    /* 
     * Name: Conversion Exception
     * Purpose: Exception thrown when input profile parameters are invalid and cannot be parsed into the correct type
     * Properties: (TextBox) callingElement = The calling textbox which threw the error
     * Input: (TextBox) callingElement = The calling textbox which threw the error
     */
    internal class ConversionException : Exception
    {
        public TextBox callingElement;
        public ConversionException(TextBox callingElement)
        {
;            this.callingElement = callingElement;
        }
    }
}

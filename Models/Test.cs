using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise.Models
{
    internal class Test
    {
        #region Properties
        private Input input;
        private Output output;
        private Flags flags;
        #endregion

        #region Constructor
        public Test(Input input, Flags flag)
        {
            this.input = input;
            this.flags = flag;
            this.output = new Output();
        }
        #endregion

        #region Getters & Setters
        public Input Input
        {
            get { return input; }
            set { input = value; }
        }

        public Output Output
        {
            get { return output; }
            set { output = value; }
        }

        public Flags Flags
        {
            get { return flags; }
            set { flags = value; }
        }
        #endregion
    }
}

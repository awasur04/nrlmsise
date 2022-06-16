using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
    internal class Output
    {
        #region Properties
        double[] densities;
        double[] temperature;
        #endregion

        #region Constructor
        public Output()
        {
            this.densities = new double[9];
            this.temperature = new double[2];
        }
        #endregion

        #region Getters & Setters
        public double[] Densities
        { 
            get { return this.densities;}
            set { this.densities = value; }
        }

        public double[] Temperature
        {
            get { return this.temperature;}
            set { this.temperature = value; }
        }
        #endregion
    }
}

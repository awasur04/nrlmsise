using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
    internal class ScientificDisplay
    {
        #region Properties
        string[] densities;
        string[] temperatures;
        #endregion

        #region Constructor
        public ScientificDisplay(Output output)
        {
            this.densities = new string[9];
            this.temperatures = new string[2];
            for (int i = 0; i < output.Densities.Length; i++)
            {
                this.densities[i] = String.Format("{0:#.#####E+0}", output.Densities[i]);
            }

            for (int i = 0; i < output.Temperature.Length; i++)
            {
                this.temperatures[i] = String.Format("{0:#.#####E+0}", output.Temperature[i]);
            }
        }
        #endregion

        #region Getters & Setters
        public string[] Densities
        {
            get { return this.densities; }
            set { this.densities = value; }
        }

        public string[] Temperature
        {
            get { return this.temperatures; }
            set { this.temperatures = value; }
        }
        #endregion
    }
}

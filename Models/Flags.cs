using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
 /*   
 *   Flags: to turn on and off particular variations use these switches.
 *   0 is off, 1 is on, and 2 is main effects off but cross terms on.
 *
 *   Standard values are 0 for switch 0 and 1 for switches 1 to 23. The 
 *   array "switches" needs to be set accordingly by the calling program. 
 *   The arrays sw and swc are set internally.
 *
 *   switches[i]:
 *    i - explanations
 *   -----------------
 *    0 - output in meters and kilograms instead of centimeters and grams
 *    1 - F10.7 effect on mean
 *    2 - time independent
 *    3 - symmetrical annual
 *    4 - symmetrical semiannual
 *    5 - asymmetrical annual
 *    6 - asymmetrical semiannual
 *    7 - diurnal
 *    8 - semidiurnal
 *    9 - daily ap [when this is set to -1 (!) the pointer
 *                  ap_a in struct nrlmsise_input must
 *                  point to a struct ap_array]
 *   10 - all UT/long effects
 *   11 - longitudinal
 *   12 - UT and mixed UT/long
 *   13 - mixed AP/UT/LONG
 *   14 - terdiurnal
 *   15 - departures from diffusive equilibrium
 *   16 - all TINF var
 *   17 - all TLB var
 *   18 - all TN1 var
 *   19 - all S var
 *   20 - all TN2 var
 *   21 - all NLB var
 *   22 - all TN3 var
 *   23 - turbo scale height var
 */
    internal class Flags
    {
        #region Properties
        int[] switches;
        double[] sw;
        double[] swc;
        #endregion

        #region Constructor
        public Flags()
        {
            this.switches = new int[24];
            this.sw = new double[24];
            this.swc = new double[24];
        }
        #endregion

        #region Getters & Setters
        public int[] Switches
        {
            get { return switches; }
            set { switches = value; }
        }

        public double[] Sw
        {
            get { return sw; }
        }

        public double[] Swc
        {
            get { return swc; }
        }
        #endregion

        #region Methods
        /*
         * Name: ComputeFlags()
         * Purpose: Set the SWC and SW array with valid switch values used for calculations
         */
        public void ComputeFlags()
        {
            for (int i = 0; i < switches.Length; i++)
            {
                //Ignore daily AP
                if (i != 9)
                {
                    //Set the sw flags
                    if (switches[i] == 1)
                    {
                        sw[i] = 1;
                    }
                    else
                    {
                        sw[i] = 0;
                    }

                    //Set the swc flag
                    if (switches[i] > 0)
                    {
                        swc[i] = 1;
                    }
                    else
                    {
                        swc[i] = 0;
                    }
                }
                //Handle daily AP flag
                else
                {
                    sw[i] = switches[i];
                    swc[i] = switches[i];
                }
            }
        }
        #endregion

    }
}

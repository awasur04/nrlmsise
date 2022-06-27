using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
    /*
     * Name: AP
     * Purpose: To model AP array if defined by user
     * Properties: (double[]) ap_array = the array of AP values defined by user
     */
    internal class AP
    {
        double[] ap_array;

        /* 
         * Array containing the following magnetic values:
        *   0 : daily AP
        *   1 : 3 hr AP index for current time
        *   2 : 3 hr AP index for 3 hrs before current time
        *   3 : 3 hr AP index for 6 hrs before current time
        *   4 : 3 hr AP index for 9 hrs before current time
        *   5 : Average of eight 3 hr AP indicies from 12 to 33 hrs 
        *           prior to current time
        *   6 : Average of eight 3 hr AP indicies from 36 to 57 hrs 
        *           prior to current time 
        */

        public AP()
        {
            this.ap_array = new double[7];
        }

        public double[] Ap_Array
        {
            get { return this.ap_array; }
            set { this.ap_array = value; }
        }
    }
}

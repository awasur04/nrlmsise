using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise.Enums
{
    /*
     * Name: ProfileMethod
     * Purpose: Represent the different profile options available for calculation
     */
    public enum ProfileMethod
    {
        ALTITUDE,
        LATITUDE,
        LONGITUDE,
        MONTH,
        DAY_OF_MONTH,
        DAY_OF_YEAR,
        HOUR_OF_DAY
    }
}

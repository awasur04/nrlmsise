using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nrlmsise.Enums;

namespace nrlmsise
{
    /*
     * Name: ProfileOption
     * Purpose: To model our profile options enabled by the calculator
     * Properties: ProfileMethod method = ProfileMethod enum to represent the method this class will model
     *             double startValue = The start value defined for the current profile method
     *             double stopValue = Stopping value defined for the current profile method
     *             double stepValue = The amount our parameter will increase after each calculation
     */
    internal class ProfileOption
    {
        #region Properties
        public ProfileMethod method { get; set; }
        public double startValue { get; set; }
        public double stopValue { get; set; }
        public double stepValue { get; set; }
        #endregion

        #region Constructor

        public ProfileOption(ProfileMethod method, double startValue, double stopValue, double stepValue)
        {
            this.method = method;
            this.startValue = startValue;
            this.stopValue = stopValue;
            this.stepValue = stepValue;
        }

        #endregion

    }
}

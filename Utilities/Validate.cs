using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nrlmsise.Enums;

namespace nrlmsise
{
    internal class Validate
    {
        #region Value Checks
        public bool Hour(string inputHour)
        {
            return compare(inputHour, 0.0, 24.0);
        }
        public bool Hour(double inputHour)
        {
            return compare(inputHour, 0.0, 24.0);
        }

        public bool Latitude(string inputLat)
        {
            return compare(inputLat, -90.0, 90.0);
        }
        public bool Latitude(double inputLat)
        {
            return compare(inputLat, -90.0, 90.0);
        }

        public bool Longitude(string inputLong)
        {
            return compare(inputLong, -180.0, 180.0);
        }
        public bool Longitude(double inputLong)
        {
            return compare(inputLong, -180.0, 180.0);
        }

        public bool Altitude(string inputAlt)
        {
            return compare(inputAlt, 0.0, 1000.0);
        }
        public bool Altitude(double inputAlt)
        {
            return compare(inputAlt, 0.0, 1000.0);
        }

        public bool F107AndAp(string inputValue)
        {
            return compare(inputValue, 0.0, 500);
        }
        public bool F107AndAp(double inputValue)
        {
            return compare(inputValue, 0.0, 500);
        }

        public bool ApFlag(string apValue)
        {
            return compare(apValue, 0, 6);
        }

        public bool Month(double month)
        {
            return compare(month, 1.0, 12.0);
        }

        public bool DayOfMonth(double inputDay)
        {
            return compare(inputDay, 1.0, 31.0);
        }

        public bool DayOfYear(double inputDay)
        {
            return compare(inputDay, 1.0, 365.0);
        }
        #endregion

        #region Profile Checks
        public bool[] Profile(double start, double stop, double step, ProfileMethod method)
        {
            //Bool Errors {START, STOP, STEP}
            bool[] profileCheck = ProfileIntervalsCheck(start, stop, step);
            bool[] noErrorArray = new bool[] { false, false, false };
            
            if (Enumerable.SequenceEqual(profileCheck, noErrorArray))
            {
                switch(method)
                {
                    case ProfileMethod.ALTITUDE:
                        if (Altitude(start))
                        {
                            if (Altitude(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.LATITUDE:
                        if (Latitude(start))
                        {
                            if (Latitude(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.LONGITUDE:
                        if (Longitude(start))
                        {
                            if (Longitude(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.MONTH:
                        if (Month(start))
                        {
                            if (Month(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.DAY_OF_MONTH:
                        if (DayOfMonth(start))
                        {
                            if (DayOfMonth(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.DAY_OF_YEAR:
                        if (DayOfYear(start))
                        {
                            if (DayOfYear(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };

                    case ProfileMethod.HOUR_OF_DAY:
                        if (Hour(start))
                        {
                            if (Hour(stop))
                            {
                                return new bool[] { false, false, false };
                            }
                            return new bool[] { false, true, false };
                        }
                        return new bool[] { true, true, false };
                }
            }
            return profileCheck;
        }

        private bool[] ProfileIntervalsCheck(double start, double stop, double step)
        {
            if (start > stop)
            {
                return new bool[] { true, true, false };
            }

            if ((step <= 0.0) || step >= Math.Abs(stop - start))
            {
                return new bool[] { false, false, true };
            }
            return new bool[] { false, false, false };
        }
        #endregion

        #region Compare
        public bool compare(string input, double minValue, double maxValue)
        {
            try
            {
                double value = Convert.ToDouble(input);
                return compare(value, minValue, maxValue);
            }
            catch
            {
                return false;
            }
        }
        public bool compare(double input, double minValue, double maxValue)
        {
            try
            {
                if ((input >= minValue) && (input <= maxValue))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}

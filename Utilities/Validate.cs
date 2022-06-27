using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nrlmsise.Enums;

namespace nrlmsise
{
    /*
     * Name: Validate
     * Purpose: To validate our input values and determine if errors are present.
     */
    internal class Validate
    {
        #region Value Checks
        /*
        * Name: Hour
        * Purpose: Check to make sure the input hour is within the valid range [0 - 23]
        * Input: (string or double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool Hour(string inputHour)
        {
            return Compare(inputHour, 0.0, 23.0);
        }
        public bool Hour(double inputHour)
        {
            return Compare(inputHour, 0.0, 23.0);
        }

        /*
        * Name: Latitude
        * Purpose: Check to make sure the input latitude is within the valid range [-90 - 90]
        * Input: (string or double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool Latitude(string inputLat)
        {
            return Compare(inputLat, -90.0, 90.0);
        }
        public bool Latitude(double inputLat)
        {
            return Compare(inputLat, -90.0, 90.0);
        }

        /*
        * Name: Longitude
        * Purpose: Check to make sure the input longitude is within the valid range [-180 - 180]
        * Input: (string or double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool Longitude(string inputLong)
        {
            return Compare(inputLong, -180.0, 180.0);
        }
        public bool Longitude(double inputLong)
        {
            return Compare(inputLong, -180.0, 180.0);
        }

        /*
        * Name: Altitude
        * Purpose: Check to make sure the input altitude is within the valid range [0 - 1000]
        * Input: (string or double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool Altitude(string inputAlt)
        {
            return Compare(inputAlt, 0.0, 1000.0);
        }
        public bool Altitude(double inputAlt)
        {
            return Compare(inputAlt, 0.0, 1000.0);
        }

        /*
        * Name: F107AndAp
        * Purpose: Check to make sure the input F107 and Ap value is within the valid range [0 - 500]
        * Input: (string or double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool F107AndAp(string inputValue)
        {
            return Compare(inputValue, 0.0, 500);
        }
        public bool F107AndAp(double inputValue)
        {
            return Compare(inputValue, 0.0, 500);
        }

        /*
        * Name: ApFlag
        * Purpose: Check to make sure the input ap flag is within the valid range [0 - 6]
        * Input: (string) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool ApFlag(string apValue)
        {
            return Compare(apValue, 0, 6);
        }

        /*
        * Name: Month
        * Purpose: Check to make sure the input month is within the valid range [1 - 12]
        * Input: (double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool Month(double month)
        {
            return Compare(month, 1.0, 12.0);
        }

        /*
        * Name: DayOfMonth
        * Purpose: Check to make sure the input day is within the valid range [1 - 31]
        * Input: (double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool DayOfMonth(double inputDay)
        {
            return Compare(inputDay, 1.0, 31.0);
        }

        /*
        * Name: DayOfYear
        * Purpose: Check to make sure the input day is within the valid range [1 - 365]
        * Input: (double) value to validate
        * Return: bool (true = valid // false = invalid)
        */
        public bool DayOfYear(double inputDay)
        {
            return Compare(inputDay, 1.0, 365.0);
        }
        #endregion

        #region Profile Checks
        /*
        * Name: Profile
        * Purpose: Validate all profile input values (start, stop, step) are valid and within the approved range
        * Input: (double) start: start value input from profile
        *        (double) stop: stop value input from profile
        *        (double) step: step value input from profile
        *        (ProfileMethod) method: The type of profile to compare for valid input
        * Return: bool[3] {Start_Error_Present, Stop_Error_Present, Step_Error_Present}  (true = error present // false = no error detected)
        */
        public bool[] Profile(double start, double stop, double step, ProfileMethod method)
        {
            //Bool Errors {START, STOP, STEP}
            bool[] noErrorArray = new bool[] { false, false, false };
            bool[] stopValueError = new bool[] { false, true, false };
            bool[] startValueError = new bool[] { true, true, false };

            bool[] profileCheck = ProfileIntervalsCheck(start, stop, step);
            

            if (Enumerable.SequenceEqual(profileCheck, noErrorArray))
            {
                switch(method)
                {
                    case ProfileMethod.ALTITUDE:
                        if (Altitude(start))
                        {
                            if (Altitude(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.LATITUDE:
                        if (Latitude(start))
                        {
                            if (Latitude(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.LONGITUDE:
                        if (Longitude(start))
                        {
                            if (Longitude(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.MONTH:
                        if (Month(start))
                        {
                            if (Month(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.DAY_OF_MONTH:
                        if (DayOfMonth(start))
                        {
                            if (DayOfMonth(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.DAY_OF_YEAR:
                        if (DayOfYear(start))
                        {
                            if (DayOfYear(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;

                    case ProfileMethod.HOUR_OF_DAY:
                        if (Hour(start))
                        {
                            if (Hour(stop))
                            {
                                return noErrorArray;
                            }
                            return stopValueError;
                        }
                        return startValueError;
                }
            }
            return profileCheck;
        }

        /*
        * Name: ProfileIntervalsCheck
        * Purpose: Validate profile intervals exist and start, stop, step values are logical
        * Input: (double) start: start value input from profile
        *        (double) stop: stop value input from profile
        *        (double) step: step value input from profile
        * Return: bool[3] {Start_Error_Present, Stop_Error_Present, Step_Error_Present}  (true = error present // false = no error detected)
        */
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
        /*
        * Name: Compare
        * Purpose: Validate profile intervals exist and start, stop, step values are logical
        * Input: (string) input: input value to validate
        *        (double) minValue: minimum acceptable value
        *        (double) maxValue: maximum acceptable value
        * Return: bool (true = valid // false = invalid)
        */
        public bool Compare(string input, double minValue, double maxValue)
        {
            try
            {
                double value = Convert.ToDouble(input);
                return Compare(value, minValue, maxValue);
            }
            catch
            {
                return false;
            }
        }

        /*
        * Name: Compare
        * Purpose: Validate profile intervals exist and start, stop, step values are logical
        * Input: (double) input: input value to validate
        *        (double) minValue: minimum acceptable value
        *        (double) maxValue: maximum acceptable value
        * Return: bool (true = valid // false = invalid)
        */
        public bool Compare(double input, double minValue, double maxValue)
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

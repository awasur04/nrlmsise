using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise
{
    internal class UIController
    {
        #region Properties
        private Flags currentTestFlags;
        private Test[][] testData;
        private ProfileOption[] currentProfileOptions;
        private Input inputParameters;
        #endregion

        #region Utility functions
        public void DefineInputParameters(DateTime inputDate, string inputHours, string inputAlt, string inputLat, string inputLong, string f107a, string f107, string ap, bool utcSelected)
        {
            inputParameters = new Input(inputDate.Year, inputDate.DayOfYear, GetSeconds(inputHours, utcSelected, inputLong), Convert.ToDouble(inputAlt), Convert.ToDouble(inputLat), Convert.ToDouble(inputLong), GetLstTime(inputHours, inputLong, utcSelected), Convert.ToDouble(f107a), Convert.ToDouble(f107), Convert.ToDouble(ap));

        }



        #region Utility functions
        public void SetTestFlags(Flags inputFlags)
        {
            testFlags = inputFlags;
        }

        public void SetApArray(double[] apValues)
        {
            for (int i = 0; i < testInputs.Length; i++)
            {
                testInputs[i].Ap_array.Ap_Array = apValues;
            }
        }

        public double GetSeconds(string inputHours, bool utc, string longitude)
        {
            //sec = hrs + (-1 * (g_long/15))
            double hours = Convert.ToDouble(inputHours);
            if (utc)
            {
                return hours * 3600;
            }
            else
            {
                double inputLong = Convert.ToDouble(longitude);

                double currentHours = hours + ((-1) * inputLong / 15);

                if (currentHours < 0.0)
                {
                    currentHours = 23.0 + currentHours;
                }

                return currentHours * 3600;
            }
        }

        public double GetLastTime(string inputHours, string longitude, bool utc)
        {
            //lst = (sec/3600) + (g_long/15)
            double hours = Convert.ToDouble(inputHours);
            if (!utc)
            {
                return hours;
            }
            else
            {
                double inputLong = Convert.ToDouble(longitude);

                double currentHours = hours + (inputLong / 15);

                if (currentHours < 0.0)
                {
                    currentHours = 23.0 + currentHours;
                }

                return currentHours;
            }
        }

        public void GetTotalTestCount(ProfileOption[] profileOptions)
        {
            testCount = 0;
            for (int i = 0; i < profileOptions.Length; i++)
            {
                double difference = Math.Abs(profileOptions[i].stopValue - profileOptions[i].startValue);
                testCount += (int)Math.Floor(difference / profileOptions[i].stepValue);
            }
            Console.WriteLine(testCount);
        }
        #endregion
    }
}

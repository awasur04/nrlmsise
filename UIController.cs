using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using nrlmsise.Enums;
using nrlmsise.Models;
using Newtonsoft.Json;

namespace nrlmsise
{
    /*
     * Name: UIController
     * Purpose: Handle interactions between UI and Logic, Models, Calculations
     * Properties: (Flags) currentTestFlags = The flags selected by user for all calculations
     *             (Test[][]) testData = All input, output, and profile data used in the calculations (Test[Profiles_Enabled_Count][Test_Per_Profile_Count])
     *             (ProfileOption[]) currentProfileOptions = All selected profile options and their data (start, stop, step, and method)
     *             (Input) inputParameters = Base input parameters before any altercations for profile options
     */
    internal class UIController
    {
        #region Properties
        private Flags currentTestFlags;
        private Test[][] testData;
        private ProfileOption[] currentProfileOptions;
        private Input inputParameters;
        #endregion

        #region Utilities
        /*
        * Name: DefineInputParameters
        * Purpose: Accept input strings from UI and parse the data into an Input object
        * Input: (DateTime) inputDate = Date from UI (Used for DayOfYear, and Year values)
        *        (string) inputHours = Used for finding the total seconds
        *        (string) inputAlt = Input altitude from ui
        *        (string) inputLat = Latitude input
        *        (string) inputLong = Longitude input
        *        (string) f107a = Average FM10.7 input
        *        (string) f107 = Daily FM10.7 input
        *        (string) ap = Magnetic AP input
        *        (bool) utcSelected = Determine which time method was used for input (true = UTC, false = LST)
        */
        public void DefineInputParameters(DateTime inputDate, string inputHours, string inputAlt, string inputLat, string inputLong, string f107a, string f107, string ap, bool utcSelected)
        {
            inputParameters = new Input(inputDate.Year, inputDate.DayOfYear, GetSeconds(inputHours, utcSelected, inputLong), Convert.ToDouble(inputAlt), Convert.ToDouble(inputLat), Convert.ToDouble(inputLong), GetLstTime(inputHours, inputLong, utcSelected), Convert.ToDouble(f107a), Convert.ToDouble(f107), Convert.ToDouble(ap));

            PopulateTestData(inputParameters);
        }

        /*
        * Name: SetTestFlags
        * Purpose: Update global inputFlags with new flag values
        * Input: (Flags) inputFlags = New input flags which will replace the current global flags set
        */
        public void SetTestFlags(Flags inputFlags)
        {
            currentTestFlags = inputFlags;
        }

        /*
        * Name: SetApArray
        * Purpose: Set testData ap_array values when an array is selected by the user
        * Input: (double[]) apValues = Values input to assign for each ap_array 
        */
        public void SetApArray(double[] apValues)
        {
            for (int i = 0; i < testData.Length; i++)
            {
                for (int j = 0; j < testData[i].Length; i++)
                {
                    testData[i][j].Input.Ap_array.Ap_Array = apValues;
                }
            }
        }

        /*
        * Name: GetSeconds
        * Purpose: Calculate the total seconds from input hours, or local apparent solar time. (Seconds = (Local_Solar_Time_Hours * 3600) + (Logitude / 15))
        * Input: (string) inputHours = Hours to convert into seconds
        *        (bool) utc = Determine if the hours are input as UTC or LST (true = UTC // false = LST)
        *        (string) logitude = Input longitude used in the calculations, only used if time is entered as LST
        * Return: (double) Total seconds converted into UTC
        */
        public double GetSeconds(string inputHours, bool utc, string longitude)
        {
            //sec = hrs + (g_long/15)
            double hours = Convert.ToDouble(inputHours);
            if (utc)
            {
                return hours * 3600;
            }
            else
            {
                double inputLong = Convert.ToDouble(longitude);

                double currentHours = hours + (inputLong / 15);

                if (currentHours < 0.0)
                {
                    currentHours = 23.0 + currentHours;
                }

                return currentHours * 3600;
            }
        }

        /*
        * Name: GetLstTime
        * Purpose: Calculate the total hours of lst time from UTC input hours. (LST_Hours = Hours_UTC - (Logitude / 15))
        * Input: (string) inputHours = Hours to convert into seconds
        *        (bool) utc = Determine if the hours are input as UTC or LST (true = UTC // false = LST)
        *        (string) logitude = Input longitude used in the calculations, only used if time is entered as LST
        * Return: (double) Total hours converted into the LST timezone
        */
        public double GetLstTime(string inputHours, string longitude, bool utc)
        {
            double hours = Convert.ToDouble(inputHours);
            if (!utc)
            {
                return hours;
            }
            else
            {
                double inputLong = Convert.ToDouble(longitude);

                double currentHours = hours - (inputLong / 15);

                if (currentHours < 0.0)
                {
                    currentHours = 23.0 + currentHours;
                }

                return currentHours;
            }
        }

        /*
        * Name: CalculateTestCount
        * Purpose: Calculate the total number of tests required given the profile options selected, also initializes the testData array to correct size
        * Input: (ProfileOption[]) profileOptions = Array of every selected profile option and the values within (start, stop, step)
        */
        public void CalculateTestCount(ProfileOption[] profileOptions)
        {
            currentProfileOptions = profileOptions;

            testData = new Test[profileOptions.Length][];

            for (int i = 0; i < profileOptions.Length; i++)
            {
                double difference = Math.Abs(profileOptions[i].stopValue - profileOptions[i].startValue);
                int testsRequired = (int)Math.Floor(difference / profileOptions[i].stepValue);

                testData[i] = new Test[testsRequired + 1];
            }
        }

        /*
        * Name: PopulateTestData
        * Purpose: Create Test objects with correct input data and flags, then store them in the global testData array.
        * Input: (Input) selectedInputParams = Parameters input by the user to be used as the base of all calculations
        */
        public void PopulateTestData(Input selectedInputParams)
        {
            if (currentProfileOptions.Length > 0)
            {
                //Every profile set
                for (int i = 0; i < testData.Length; i++)
                {
                    ProfileOption selectedTestOptions = currentProfileOptions[i];
                    int k = 0;

                    for (double j = selectedTestOptions.startValue; j <= selectedTestOptions.stopValue; j += selectedTestOptions.stepValue)
                    {
                        testData[i][k] = new Test((Input)selectedInputParams.Clone(), currentTestFlags);
                        CorrectProfileInputValues(selectedTestOptions.method, testData[i][k++].Input, j);
                    }
                }
            }
            else
            {
                testData = new Test[1][];
                testData[0] = new Test[] { new Test((Input)selectedInputParams.Clone(), currentTestFlags)};
            } 
        }

        /*
        * Name: CorrectProfileInputValues
        * Purpose: Change select input value to calculate for the profile options chosen by user
        * Input: (ProfileMethod) selectedMethod = Enum which represents the current input value to change
        *        (Input) input = The current input data to change
        *        (double) value = The new value which will replace our current input value
        */
        public void CorrectProfileInputValues(ProfileMethod selectedMethod, Input input, double value)
        {
            DateTime currentDate;
            DateTime newStartDate;

            switch(selectedMethod)
            {
                case ProfileMethod.ALTITUDE:
                    input.Altitude = value;
                    break;

                case ProfileMethod.LATITUDE:
                    input.G_lat = value;
                    break;

                case ProfileMethod.LONGITUDE:
                    input.G_long = value;
                    break;

                case ProfileMethod.MONTH:
                    currentDate = new DateTime(input.Year, 1, 1).AddDays(input.DayOfYear - 1);
                    newStartDate = new DateTime(input.Year, (int)value, currentDate.Day);
                    input.DayOfYear = newStartDate.DayOfYear;
                    break;

                case ProfileMethod.DAY_OF_MONTH:
                    currentDate = new DateTime(input.Year, 1, 1).AddDays(input.DayOfYear - 1);
                    newStartDate = new DateTime(input.Year, currentDate.Month, (int)value);
                    input.DayOfYear = newStartDate.DayOfYear;
                    break;

                case ProfileMethod.DAY_OF_YEAR:
                    input.DayOfYear = (int)value;
                    break;

                case ProfileMethod.HOUR_OF_DAY:
                    currentDate = new DateTime(input.Year, 1, 1).AddDays(input.DayOfYear - 1);
                    newStartDate = new DateTime(input.Year, currentDate.Month, currentDate.Day).AddHours(value);
                    input.Seconds = newStartDate.TimeOfDay.TotalSeconds;
                    break;
            }
        }

        /*
        * Name: RunTests
        * Purpose: Run calculations for every Test in testData
        */
        public void RunTests()
        {
            for (int i = 0; i < testData.Length; i++)
            {
                for (int j = 0; j < testData[i].Length; j++)
                {
                    testData[i][j].Output = Calculations.Run(testData[i][j]);
                }
            }
        }

        /*
        * Name: ExportResultsToJson
        * Purpose: Serialize JSONOutput object which will represent all testData, then write to the desired path
        * Input: (string) path = Path where the json file will be saved
        */
        public void ExportResultsToJson(string path)
        {
            string jsonOutput = JsonConvert.SerializeObject(new JSONOutput(currentProfileOptions, testData, inputParameters), Formatting.Indented);
            System.IO.File.WriteAllText(GetPath(path + "\\output.json"), jsonOutput);
        }

        /*
        * Name: ExportResultsToGraph
        * Purpose: Pass all test, input and profile data to the GraphResults form, then set it active to display to the user.
        */
        public void ExportResultsToGraph()
        {
            var graphForm = new GraphResults(inputParameters, testData, currentProfileOptions);
            graphForm.Show();
        }

        /*
        * Name: GetPath
        * Purpose: Check if the json file already exists, if it does add a digit to the file name until an unused name is found. (prevent overwriting data)
        * Input: (string) path = Path the user wants to save their json file
        */
        public string GetPath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path);

            for (int i = 0; ; i++)
            {
                if (!File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(directory, fileName + "_" + i + fileExtension);
            }
        }
        #endregion
    }
}

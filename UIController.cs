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

            PopulateTestData(inputParameters);
        }

        public void SetTestFlags(Flags inputFlags)
        {
            currentTestFlags = inputFlags;
        }

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

        public double GetLstTime(string inputHours, string longitude, bool utc)
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

        public void ExportResultsToJson(string path)
        {
            string jsonOutput = JsonConvert.SerializeObject(new JSONOutput(currentProfileOptions, testData, inputParameters), Formatting.Indented);
            System.IO.File.WriteAllText(GetPath(path + "\\output.json"), jsonOutput);
        }

        public void ExportResultsToGraph()
        {
            var graphForm = new GraphResults(inputParameters, testData, currentProfileOptions);
            graphForm.Show();
        }

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

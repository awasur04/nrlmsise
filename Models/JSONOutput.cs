using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nrlmsise.Models
{
    /* 
     * Name: JSONOutput
     * Purpose: Model to represent readable JSON format for all test outputs as well as the input parameters used in calculations.
     * Input Parameters: (ProfileOption[]) profileOptions = All the profile options used for the current calculations
     *                   (Test[][]) testData = All test data used for calclations (Inludes Input, Output, and Flags)
     *                   (Input) defaultInputParameters = Parameters entered by the user before calculations.
     */
    internal class JSONOutput
    {
        public Input inputParameters;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JSONOutputProfile[] ProfileOutputs;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JSONTestOutput TestOutput;

        public JSONOutput(ProfileOption[] profileOptions, Test[][] testData, Input defaultInputParameters)
        {
            inputParameters = defaultInputParameters;

            if (profileOptions.Length > 0)
            {
                ProfileOutputs = new JSONOutputProfile[testData.Length];
                for (int i = 0; i < testData.Length; i++)
                {
                    ProfileOutputs[i] = new JSONOutputProfile(profileOptions[i], testData[i]);
                }
            }
            else
            {
                TestOutput = new JSONTestOutput(testData[0][0].Output);
            }
        }
    }

    /* 
     * Name: JSONOutputProfile
     * Purpose: Model to represent readable JSON format for only tests performed by the specified profile option
     * Input Parameters: (ProfileOption) profile = Current profile options used in the given set of data
     *                   (Test[]) currentTest = Test data used and calculated for the specified profile
     */
    internal class JSONOutputProfile
    {
        public string ProfileOptions;
        public Dictionary<string, JSONTestOutput> TestResults;

        public JSONOutputProfile(ProfileOption profile, Test[] currentTest)
        {
            double currentValue = profile.startValue;

            ProfileOptions = profile.method.ToString() + "(" + profile.startValue + ", " + profile.stopValue + ", " + profile.stepValue + ")";
            TestResults = new Dictionary<string, JSONTestOutput>();

            for (int i = 0; i < currentTest.Length; i++)
            {
                if (currentValue <= profile.stopValue)
                {
                    TestResults.Add(profile.method.ToString() + " " + currentValue, new JSONTestOutput(currentTest[i].Output));
                    currentValue += profile.stepValue;
                }
            }
        }
    }

    /* 
     * Name: JSONTestOuput
     * Purpose: Model to represent readable JSON format for the specified Test object
     * Input Parameters: (Output) testOutput = Output calculated for the current Test object
     */
    internal class JSONTestOutput
    {
        //Add scientifc display
        public Dictionary<string, string> Results;

        public JSONTestOutput(Output testOutput)
        {
            Results = new Dictionary<string, string>
            {
                { "TINF", ConvertToScientific(testOutput.Temperature[0]) + " K" },
                { "TG", ConvertToScientific(testOutput.Temperature[1]) + " K" },
                { "HE", ConvertToScientific(testOutput.Densities[0]) + " CM3"},
                { "O", ConvertToScientific(testOutput.Densities[1]) + " CM3"},
                { "N2", ConvertToScientific(testOutput.Densities[2]) + " CM3"},
                { "O2", ConvertToScientific(testOutput.Densities[3]) + " CM3"},
                { "AR", ConvertToScientific(testOutput.Densities[4]) + " CM3"},
                { "H", ConvertToScientific(testOutput.Densities[6]) + " CM3"},
                { "N", ConvertToScientific(testOutput.Densities[7]) + " CM3"},
                { "ANM", ConvertToScientific(testOutput.Densities[8]) + " CM3"},
                { "RHO", ConvertToScientific(testOutput.Densities[5]) + " GM/CM3"}
            };
        }

        /* 
        * Name: ConvertToScientific
        * Purpose: Method to convert given double values into scientifc notation (1000 -> 1.0E+3)
        * Input Parameters: (double) value = Value to convert into scientific notation
        * Return: (string) - scientific representation of the given value
        */
        public string ConvertToScientific(double value)
        {
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                return "Not Available";
            }
            return String.Format("{0:#.#####E+0}", value);
        }
    }
}

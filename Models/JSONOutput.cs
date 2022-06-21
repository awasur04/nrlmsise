using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise.Models
{
    internal class JSONOutput
    {
        public Input inputParameters;
        public JSONOutputProfile[] ProfileOutputs;

        public JSONOutput(ProfileOption[] profileOptions, Test[][] testData, Input defaultInputParameters)
        {
            inputParameters = defaultInputParameters;
            ProfileOutputs = new JSONOutputProfile[testData.Length];

            for (int i = 0; i < testData.Length; i++)
            {
                ProfileOutputs[i] = new JSONOutputProfile(profileOptions[i], testData[i]);
            }
        }
    }

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

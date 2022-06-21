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
                { "TINF", ConvertToScientific(testOutput.Temperature[0]) },
                { "TG", ConvertToScientific(testOutput.Temperature[1]) },
                { "HE", ConvertToScientific(testOutput.Densities[0]) },
                { "O", ConvertToScientific(testOutput.Densities[1]) },
                { "N2", ConvertToScientific(testOutput.Densities[2]) },
                { "O2", ConvertToScientific(testOutput.Densities[3]) },
                { "AR", ConvertToScientific(testOutput.Densities[4]) },
                { "H", ConvertToScientific(testOutput.Densities[6]) },
                { "N", ConvertToScientific(testOutput.Densities[7]) },
                { "ANM", ConvertToScientific(testOutput.Densities[8]) },
                { "RHO", ConvertToScientific(testOutput.Densities[5]) }
            };
        }

        public string ConvertToScientific(double value)
        {
            return String.Format("{0:#.#####E+0}", value);
        }
    }
}

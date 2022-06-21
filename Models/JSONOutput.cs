using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nrlmsise.Models
{
    internal class JSONOutput
    {
        Input inputParameters;
        JSONOutputProfile[] ProfileOutputs;

        public JSONOutput(ProfileOption[] profileOptions, Test[][] testData)
        {

        }
    }

    internal class JSONOutputProfile
    {
        string ProfileOption;
        JSONTest Tests;

    }

    internal class JSONTest
    {
        Dictionary<string, JSONTestOutput> TestResults;
    }

    internal class JSONTestOutput
    {
        Dictionary<string, string> Results;

        public JSONTestOutput(Output testOutput)
        {
            Results = new Dictionary<string, string>
            {
                { "TINF", testOutput.Temperature[0].ToString() },
                { "TG", testOutput.Temperature[1].ToString() },
                { "HE", testOutput.Densities[0].ToString() },
                { "O", testOutput.Densities[1].ToString() },
                { "N2", testOutput.Densities[2].ToString() },
                { "O2", testOutput.Densities[3].ToString() },
                { "AR", testOutput.Densities[4].ToString() },
                { "H", testOutput.Densities[6].ToString() },
                { "N", testOutput.Densities[7].ToString() },
                { "ANM", testOutput.Densities[8].ToString() },
                { "RHO", testOutput.Densities[5].ToString() }
            };
        }
    }
}

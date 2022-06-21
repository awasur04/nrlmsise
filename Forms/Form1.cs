using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using nrlmsise.Enums;
using nrlmsise.Exceptions;

namespace nrlmsise
{
    public partial class Form1 : Form
    {
        #region Properties
        private CheckBox[] flagBoxes;
        private TextBox[] inputParams;

        private string outputPath;
        private ProfileOption[] enabledProfileOption;
        private UIController uiController;
        private Validate validate = new Validate();
        #endregion

        #region Constructor
        public Form1()
        {
            this.uiController = new UIController();

            InitializeComponent();
            statusLabel.Text = "Initializing";

            DefaultUISettings();
            CreateUiArrays();
            statusLabel.Text = "Ready";
        }
        #endregion

        #region Button Event Handlers
        private void EnablallFlagButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < flagBoxes.Length; i++)
            {
                flagBoxes[i].Checked = true;
            }
            dailyApFlagTextbox.Text = "1";
        }

        private void DisableallFlagButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < flagBoxes.Length; i++)
            {
                flagBoxes[i].Checked = false;
            }
            dailyApFlagTextbox.Text = "0";
        }

        private void ResetFormButton_Click(object sender, EventArgs e)
        {
            ResetForm(inputBox);

            dateTimePicker1.Value = DateTime.Parse("6/21/2021 2:38 PM");
            utcRadioButton.Checked = true;
            lastRadioButton.Checked = false;
            statusLabel.Text = "Ready";
        }

        private void directoryButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                outputPath = folderBrowserDialog1.SelectedPath;
                directoryTextbox.Text = outputPath;
            }
        }

        private void GengraphButton_Click(object sender, EventArgs e)
        {
            Submit(1);
        }

        private void ExpjsonButton_Click(object sender, EventArgs e)
        {
            //Calculations.TestGTD7();
            Submit(0);
        }
        #endregion

        #region Utilities

        private void DefaultUISettings()
        {
            //Set Deafult Directory
            directoryTextbox.Text = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            outputPath = directoryTextbox.Text;

        }
        private void Submit(int method)
        {
            // 0: JSON
            // 1: Graph

            statusLabel.Text = "Validating input";
            if (ValidateInput())
            {
                Flags inputFlags = GetInputFlags();
                if (inputFlags != null)
                {
                    statusLabel.Text = "Calculating test data";
                    uiController.SetTestFlags(inputFlags);
                    uiController.CalculateTestCount(enabledProfileOption);
                    uiController.DefineInputParameters(dateTimePicker1.Value, hodTextbox.Text, altTextbox.Text, latTextbox.Text, longTextbox.Text, f107aTextbox.Text, f107Textbox.Text, apTextbox.Text, utcRadioButton.Checked);
                    
                    statusLabel.Text = "Running simulations";
                    uiController.RunTests();

                    statusLabel.Text = "Calculations complete";

                    if (method == 0)
                    {
                        uiController.ExportResultsToJson(outputPath);
                    }

                    //Cleaup current test arrays
                    enabledProfileOption = null;

                }
            }
        }

        private void CreateUiArrays()
        {
            inputParams = new TextBox[] { hodTextbox, latTextbox, longTextbox, altTextbox, f107Textbox, f107aTextbox, apTextbox };

            flagBoxes = new CheckBox[] { tn3FlagBox, nlbFlagBox, tn2FlagBox, sFlagBox, tn1FlagBox, tlbFlagBox, tinfFlagBox, turboFlagBox, mixedFlagBox, utmixedFlagBox, longFlagBox, 
                diffequFlagBox, teriFlagBox, utlongFlagBox, semidiFlagBox, diurnalFlagBox, asymannFlagBox, asymsemiFlagBox, symannFlagBox, symsemiFlagBox, timeFlagBox, f107FlagBox};
        }

        private bool ValidateInput()
        {
            if (ValidateInputParameters())
            {
                enabledProfileOption = GetActiveProfileOptions();
                if (enabledProfileOption != null && ValidateProfileOptions(enabledProfileOption))
                {
                    return true;
                }
                return false;
                
            }
            return false;
        }

        private bool ValidateInputParameters()
        {
            bool[] errorIndex = new bool[7];
            int errorCount = 0;

            for (int i = 0; i < inputParams.Length; i++)
            {
                Control uiElement = inputParams[i];
                switch(uiElement.Name)
                {
                    case "hodTextbox":
                        errorIndex[i] = validate.Hour(inputParams[i].Text);
                        break;

                    case "latTextbox":
                        errorIndex[i] = validate.Latitude(inputParams[i].Text);
                        break;

                    case "longTextbox":
                        errorIndex[i] = validate.Longitude(inputParams[i].Text);
                        break;

                    case "altTextbox":
                        errorIndex[i] = validate.Altitude(inputParams[i].Text);
                        break;

                    case "apTextbox":
                    case "f107aTextbox":
                    case "f107Textbox":
                        errorIndex[i] = validate.F107AndAp(inputParams[i].Text);
                        break;
                }
            }

            for (int i = 0; i < errorIndex.Length; i++)
            {
                if (errorIndex[i] == false)
                {
                    errorCount++;
                    inputParams[i].ForeColor = Color.Red;
                }
                else
                {
                    inputParams[i].ForeColor = Color.Black;
                }
            }

            if (errorCount != 0)
            {
                statusLabel.Text = "Errors in input parameters";
                MessageBox.Show("You have " + errorCount + " invalid input parameters");
                return false;
            }
            return true;
        }

        private bool ValidateProfileOptions(ProfileOption[] enabledOptions)
        {
            bool[][] errorIndex = new bool[tabControl1.TabCount][];
            int errorCount = 0;

            for (int i = 0; i < enabledOptions.Length; i++)
            {
                ProfileOption currentOption = enabledOptions[i];
                int errorTabIndex = (int)currentOption.method;
                errorIndex[errorTabIndex] = validate.Profile(currentOption.startValue, currentOption.stopValue, currentOption.stepValue, currentOption.method);

                if ((errorIndex[errorTabIndex][0] == true) || (errorIndex[errorTabIndex][1] == true) || (errorIndex[errorTabIndex][2] == true))
                {
                    errorCount++;
                }
            }

            for (int i = 0; i < errorIndex.Length; i++)
            {
                foreach (Control control in tabControl1.TabPages[i].Controls)
                {
                    if ( (control is TextBox tb) && (errorIndex[i] != null) )
                    {
                        switch(tb.Name)
                        {
                            case string a when a.Contains("Start"):
                                ToggleError(tb, errorIndex[i][0]);
                                break;

                            case string b when b.Contains("Stop"):
                                ToggleError(tb, errorIndex[i][1]);
                                break;

                            case string c when c.Contains("Step"):
                                ToggleError(tb, errorIndex[i][2]);
                                break;
                        }
                    }
                }
            }

            if (errorCount != 0)
            {
                statusLabel.Text = "Errors in profile options";
                MessageBox.Show("You have " + errorCount + " invalid profile options" +
                    "\nPlease make sure the profile parameters fall within the required range." +
                    "\nAlso ensure that the step value is smaller than the profile range.");
                return false;
            }
            return true;
        }

        private ProfileOption[] GetActiveProfileOptions()
        {
            ProfileOption[] possibleProfileOptions = new ProfileOption[7];
            int pagesEnabled = 0;

            TabControl.TabPageCollection tabPages = tabControl1.TabPages;

            for (int i = 0; i < tabPages.Count; i++)
            {
                TabPage currentPage = tabPages[i];
                if (IsPageEnabled(currentPage))
                {
                    try
                    {
                        double[] profileOption = GetProfileValues(currentPage);

                        possibleProfileOptions[pagesEnabled++] = new ProfileOption((ProfileMethod)i, profileOption[0], profileOption[1], profileOption[2]);
                    }
                    catch(ConversionException ce)
                    {
                        ToggleError(ce.callingElement, true);
                        statusLabel.Text = "Errors in profile options";
                        MessageBox.Show("Please fix the highlighted input value");
                        return null;
                    }
                }
            }
            //Remove all empty indices from our array before returning
            return possibleProfileOptions.Where(profile => profile != null).ToArray();
        }

        private bool IsPageEnabled(TabPage tabPage)
        {
            foreach (Control c in tabPage.Controls)
            {
                if (c is CheckBox checkBox)
                {
                    return checkBox.Checked;
                }
            }
            return false;
        }

        private double[] GetProfileValues(TabPage tabPage)
        {
            double[] values = new double[3];

            foreach (Control control in tabPage.Controls)
            {
                if (control is TextBox textBox)
                {
                    try
                    {
                        switch (textBox.Name)
                        {
                            case string a when a.Contains("Start"):
                                values[0] = Convert.ToDouble(textBox.Text);
                                ToggleError(textBox, false);
                                break;

                            case string b when b.Contains("Stop"):
                                values[1] = Convert.ToDouble(textBox.Text);
                                ToggleError(textBox, false);
                                break;

                            case string c when c.Contains("Step"):
                                values[2] = Convert.ToDouble(textBox.Text);
                                ToggleError(textBox, false);
                                break;
                        }
                    }
                    catch
                    {
                        throw new ConversionException(textBox);
                    }
                }
            }
            return values;
        }

        private Flags GetInputFlags()
        {
            Flags selectedFlags = new Flags();

            for (int i = 0; i < variationPanel.Controls.Count; i++)
            {
                if (variationPanel.Controls[i] is CheckBox cb)
                {
                    int tabIndex = cb.TabIndex;
                    selectedFlags.Switches[tabIndex] = Convert.ToInt32(cb.Checked);
                }
            }

            string apInput = dailyApFlagTextbox.Text;
            if (validate.ApFlag(apInput))
            {
                ToggleError(dailyApFlagTextbox, false);
                switch (apInput.Length)
                {
                    case 0:
                        selectedFlags.Switches[9] = 0;
                        break;
                    case 1:
                        selectedFlags.Switches[9] = Convert.ToInt32(apInput);
                        break;
                    default:
                        selectedFlags.Switches[9] = -1;
                        uiController.SetApArray(Array.ConvertAll(apInput.Split(','), double.Parse));
                        break;
                }
            }
            else
            {
                statusLabel.Text = "Invalid flag input";
                MessageBox.Show("Daily ap flag is invalid");
                ToggleError(dailyApFlagTextbox, true);
                return null;
            }
            return selectedFlags;
            
        }

        private void ToggleError(TextBox tb, bool errorPresent)
        {
            if (errorPresent)
            {
                tb.ForeColor = Color.Red;
            }
            else
            {
                tb.ForeColor = Color.Black;
            }
        }

        private void ResetForm(Control container)
        {
            foreach (Control control in container.Controls)
            {
                ResetControl(control);
            }

            if (container.Name == "inputBox")
            {
                ResetForm(tabControl1);
            }
            else if (container.Name == "tabControl1")
            {
                ResetForm(variationPanel);
            }
            enabledProfileOption = null;
        }

        public void ResetControl(Control control)
        {
            switch (control)
            {
                case TextBox tb:
                    tb.Text = "";
                    ToggleError(tb, false);
                    break;

                case CheckBox cb:
                    cb.Checked = false;
                    break;

                case TabPage tabPage:
                    ResetForm(tabPage);
                    break;
            }
        }
        #endregion  
    }
}

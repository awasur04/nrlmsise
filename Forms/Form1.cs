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
    /*
     * Name: Form1
     * Extends: Form
     * Purpose: Main UI and utility/event methods
     * Properties: (CheckBox[]) flagBoxes = All CheckBoxs which are included in the Model Flags section of the UI
     *             (TextBox[]) inputParams = All input TextBoxs which are included in the Input Parameters section of the UI.
     *             (string) outputPath = Current selected output path for json export (Default: Desktop)
     *             (ProfileOption[]) enabledProfileOption = All profile options which have been enabled in the UI
     *             (UIController) uiController = UIController class used in communicating with other classes in the program
     *             (Validate) validate = Class used to validate input data before passing it to the UIController
     */
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
        /*
        * Name: Form1
        * Purpose: Define instances of uiCOntroller, and setup ui elements
        */
        public Form1()
        {
            this.uiController = new UIController();

            InitializeComponent();
            statusLabel.Text = "Initializing";

            CreateUiArrays();
            DefaultUISettings();
            statusLabel.Text = "Ready";
        }
        #endregion

        #region Button Event Handlers
        /*
        * Name: EnablallFlagButton_Click
        * Purpose: Enable all flag CheckBoxs inside the Model Flags group
        * Event: Button click on enablallFlagButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void EnablallFlagButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < flagBoxes.Length; i++)
            {
                flagBoxes[i].Checked = true;
            }
            dailyApFlagTextbox.Text = "1";
        }

        /*
        * Name: DisableallFlagButton_Click
        * Purpose: Disable all flag CheckBoxs inside the Model Flags group
        * Event: Button click on disableallFlagButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void DisableallFlagButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < flagBoxes.Length; i++)
            {
                flagBoxes[i].Checked = false;
            }
            dailyApFlagTextbox.Text = "0";
        }

        /*
        * Name: ResetFormButton_Click
        * Purpose: Clear all controls and data within the form
        * Event: Button click on resetFormButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void ResetFormButton_Click(object sender, EventArgs e)
        {
            ResetForm(inputBox);

            dateTimePicker1.Value = DateTime.Parse("6/21/2021 2:38 PM");
            utcRadioButton.Checked = true;
            lastRadioButton.Checked = false;
            statusLabel.Text = "Ready";
        }

        /*
        * Name: directoryButton_Click
        * Purpose: Open the folderBrowserDialog to select another output path, validates selected path before updating outputPath.
        * Event: Button click on directoryButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void directoryButton_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                outputPath = folderBrowserDialog1.SelectedPath;
                directoryTextbox.Text = outputPath;
            }
        }

        /*
        * Name: GengraphButton_Click
        * Purpose: Once all input is validated pass data to GraphResults and display the results form.
        * Event: Button click on gengraphButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void GengraphButton_Click(object sender, EventArgs e)
        {
            Submit(1);
        }

        /*
        * Name: ExpjsonButton_Click
        * Purpose: Once all input is validated pass data JSONOutput to and create a new file.
        * Event: Button click on expjsonButton
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (EventArgs) e = Provides information on the current event data.
        */
        private void ExpjsonButton_Click(object sender, EventArgs e)
        {
            //Calculations.TestGTD7();
            Submit(0);
        }
        #endregion

        #region Utilities
        /*
        * Name: DefaultUISettings
        * Purpose: Setup UI with desired values (Define default output path, and enable all flag checkboxs)
        */
        private void DefaultUISettings()
        {
            //Set Deafult Directory
            directoryTextbox.Text = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            outputPath = directoryTextbox.Text;
            EnablallFlagButton_Click(null, null);
        }

        /*
        * Name: Submit
        * Purpose: Validate all input data then pass it to the UIController and call the corresponding output method.
        * Input: (int) method: Current selected method for submitting the data (0 = Export to JSON // 1 = Graph)
        */ 
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
                    else
                    {
                        uiController.ExportResultsToGraph();
                    }

                    //Cleaup current test arrays
                    enabledProfileOption = null;

                }
            }
        }

        /*
        * Name: CreateUiArrays
        * Purpose: Populate inputParams, and flagBoxes array with all corresponding UI Controls
        */
        private void CreateUiArrays()
        {
            inputParams = new TextBox[] { hodTextbox, latTextbox, longTextbox, altTextbox, f107Textbox, f107aTextbox, apTextbox };

            flagBoxes = new CheckBox[] { tn3FlagBox, nlbFlagBox, tn2FlagBox, sFlagBox, tn1FlagBox, tlbFlagBox, tinfFlagBox, turboFlagBox, mixedFlagBox, utmixedFlagBox, longFlagBox, 
                diffequFlagBox, teriFlagBox, utlongFlagBox, semidiFlagBox, diurnalFlagBox, asymannFlagBox, asymsemiFlagBox, symannFlagBox, symsemiFlagBox, timeFlagBox, f107FlagBox};
        }

        /*
        * Name: ValidateInput
        * Purpose: Validate all input and profile data given by user.
        * Return: (bool) Value represents input validity (true = input is valid // false = input invalid)
        */
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

        /*
        * Name: ValidateInputParameters
        * Purpose: Validate input parameters with corresponding validate method, if errors are present change textBox color to red and display a message to the user
        * Return: (bool) Value represents input validity (true = input is valid // false = input invalid)
        */
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

        /*
        * Name: ValidateProfileOptions
        * Purpose: Validate enabled profile options with corresponding validate method, if errors are present change textBox color to red and display a message to the user
        * Input: (ProfileOption[]) enabledOptions = All profile options which have been enabled and the data contained within them. (start, stop, step, method)
        * Return: (bool) Value represents profile options validity (true = input is valid // false = input invalid)
        */
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
                MessageBox.Show("You have " + errorCount + " invalid profile options." +
                    "\nPlease make sure the profile parameters fall within the required range." +
                    "\nAlso ensure that the step value is smaller than the profile range.");
                return false;
            }
            return true;
        }

        /*
        * Name: GetActiveProfileOptions
        * Purpose: Get all enabled profile options from the UI, create a new ProfileOption for each option enabled
        * Return: (ProfileOption[]) Array containing all enabled profile options and their data
        */
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
                        MessageBox.Show("You have invalid parameters in your profile options." +
                            "\nPlease correct the highlighted values.");
                        return null;
                    }
                }
            }
            //Remove all empty indices from our array before returning
            return possibleProfileOptions.Where(profile => profile != null).ToArray();
        }

        /*
        * Name: IsPageEnabled
        * Purpose: Determines if the profile option enabledCheckBox is current selected
        * Return: (bool) Current status of the checkbox (true = enabled // false = disabled)
        */
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

        /*
        * Name: GetProfileValues
        * Purpose: If the profile option is enabled, then gather the start, stop, and step values from the corresponding text boxes
        * Input: (TabPage) Selected tabPage to gather data from
        * Return: (double[]) Data retrieved from the input TabPage [start, stop, step]
        */
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

        /*
        * Name: GetInputFlags
        * Purpose: Determine all enabled flags from Model Flags group and set the flag switch value to the corresponding bool value
        * Return: (Flags) Flags model with the correct switch values
        */
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

        /*
        * Name: ToggleError
        * Purpose: Set the textbox color to red, or black for the input textbox
        * Input: (TextBox) tb = Current TextBox to change the color of.
        *        (bool) errorPresent = Whether an error is present or not (true = error present // false = no error present)
        */
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

        /*
        * Name: ResetForm
        * Purpose: Reset all controls in the given container to be unselected and blank
        * Input: (Control) container = Container to iterate for all of the controls and reset their values
        */
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

        /*
        * Name: ResetControl
        * Purpose: Reset the given control based on its type
        * Input: (Control) control = Current control to reset
        */
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

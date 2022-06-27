using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using nrlmsise.Models;
using nrlmsise.Enums;
using Newtonsoft.Json;

namespace nrlmsise
{
    /*
     * Name: GraphResults
     * Extends: Form
     * Purpose: Graph calculation output data onto a seperate form(UI).
     * Properties: (Input) initialInput = Base input parameters before any altercations for profile options
     *             (Test[][]) testData = All input, output, and profile data used in the calculations (Test[Profiles_Enabled_Count][Test_Per_Profile_Count])
     *             (ProfileOption[]) profileOptions = All selected profile options and their data (start, stop, step, and method)
     *             (string[]) outputVariables = Labels used for creating individual tabs for each calculation element.
     */
    internal partial class GraphResults : Form
    {
        #region Global Properties
        Input initialInput;
        Test[][] testData;
        ProfileOption[] profileOptions;

        string[] outputVariables = new string[] { "HELIUM (HE)", "ATOMIC OXYGEN (O)", "NITROGEN (N2)", "OXYGEN (O2)", "ARGON (AR)", "TOTAL MASS DENSITY (GM/CM3)", "HYDROGEN (H)", "NITROGEN (N)", "ANON. O. DENSITY", "EXOSPHERIC TEMP.", "TEMP. AT ALT." };
        #endregion

        #region Constructor
        /*
        * Name: GraphResults
        * Purpose: Create a new GraphResults form and initialize all data required.
        * Input: (Input) initialInput = Base input parameters before any altercations for profile options
        *        (Test[][]) testData = All input, output, and profile data used in the calculations (Test[Profiles_Enabled_Count][Test_Per_Profile_Count])
        *        (ProfileOption[]) profileOptions = All selected profile options and their data (start, stop, step, and method)
        */
        public GraphResults(Input initialInput, Test[][] testData, ProfileOption[] profileOptions)
        {
            this.initialInput = initialInput;
            this.testData = testData;
            this.profileOptions = profileOptions;

            InitializeComponent();
            DisplayInputParameters();
            CreateProfileTabs();
        }
        #endregion

        #region Event Handlers
        /*
        * Name: Chart_MouseClick
        * Purpose: Update closest point when a user clicks on the graph.
        * Event: Mouse click on all charts.
        * Input: (object) sender = The UI componenet which triggered the event call
        *        (MouseEventArgs) e = Provides data on the current mouse actions and location.
        */
        private void Chart_MouseClick(object sender, MouseEventArgs e)
        {
            Chart selectedChart = (Chart)sender;

            HitTestResult hitResult = selectedChart.HitTest(e.X, e.Y);

            if ( hitResult != null && hitResult.PointIndex >= 0)
            {
                //CHart -> IndividualTab -> IndividualTabControl -> ProfileTabPage
                TabPage profileTabPage = (TabPage)selectedChart.Parent.Parent.Parent;

                if (profileTabPage != null)
                {
                    for (int i = 0; i < profileTabPage.Controls.Count; i++)
                    {
                        if (profileTabPage.Controls[i].Name == "selectionGroupBox")
                        {
                            GroupBox selectedTexGroupBox = (GroupBox)profileTabPage.Controls[i];
                            for (int j = 0; j < selectedTexGroupBox.Controls.Count; j++)
                            {
                                switch (selectedTexGroupBox.Controls[j].Name)
                                {
                                    case "xLabel":
                                        selectedTexGroupBox.Controls[j].Text = "X: " + hitResult.Series.Points[hitResult.PointIndex].XValue + GetXUnitsFromParent(profileTabPage.Text);
                                        break;

                                    case "yLabel":
                                        selectedTexGroupBox.Controls[j].Text = "Y: " + ReverseLogForm(hitResult.Series.Points[hitResult.PointIndex].YValues[0]) + GetYUnitsFromParent(selectedChart.Parent.Text); ;
                                        break;

                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Creation Methods
        /*
        * Name: DisplayInputParameters
        * Purpose: Fill the inputParametersLabel with initialInput defined in global properties.
        */
        public void DisplayInputParameters()
        {
            string inputData = JsonConvert.SerializeObject(this.initialInput, Formatting.Indented);
            inputParametersLabel.Text = inputData;
        }

        /*
        * Name: CreateProfileTabs
        * Purpose: For each profile option selected create a new TabPage and add it to our main TabControl
        */
        public void CreateProfileTabs()
        {
            for (int i = 0; i < profileOptions.Length; i++)
            {
                TabPage tp = new TabPage(profileOptions[i].method.ToString() + "TabPage");

                tp.Controls.Add(CreateProfileGroupBox(profileOptions[i]));
                tp.Controls.Add(CreateSelectionBox());
                tp.Controls.Add(CreateProfileTabControls(profileOptions[i], i));

                tp.Text = profileOptions[i].method.ToString();
                tp.Location = new System.Drawing.Point(4, 22);
                tp.Size = new System.Drawing.Size(1175, 558);
                tp.UseVisualStyleBackColor = true;

                tabControl1.TabPages.Add(tp);
            }
        }

        /*
        * Name: CreateProfileTabControls
        * Purpose: Inside of each profile tab create a new tabcontrol, then create a tab page for element in ouputVariables (11)
        * Input: (ProfileOption) profileOption = the current profile option which is being used to create a new TabControl
        *        (int) profileIndex = Current index of the selected profile option in global profileOptions. (Not used in this method, but important to pass to CreateProfileChart() for retrieving the correct output)
        * Return: (TabControl) Complete tabcontrol for the passed profile option with charts and seperate tabs for each element.
        */
        public TabControl CreateProfileTabControls(ProfileOption profileOption, int profileIndex)
        {
            TabControl profileTabControl = new TabControl();
            profileTabControl.Location = new System.Drawing.Point(7, 74);
            profileTabControl.SelectedIndex = 0;
            profileTabControl.Size = new System.Drawing.Size(1161, 478);

            for (int i = 0; i < outputVariables.Length; i++)
            {
                TabPage profileStepPage = new TabPage(outputVariables[i]);
                profileStepPage.Location = new System.Drawing.Point(7, 74);
                profileStepPage.Size = new System.Drawing.Size(1153, 452);

                //Each step page needs a chart
                profileStepPage.Controls.Add(CreateProfileChart(i, profileIndex));

                profileTabControl.TabPages.Add(profileStepPage);
            }
            
            return profileTabControl;
        }

        /*
        * Name: CreateProfileChart
        * Purpose: Create a chart for the given output data and plot each point.
        * Input: (int) outputIndex = Current index which corresponds with outputVariables for retrieving correct data from output.
        *        (int) profileIndex = Current index of the selected profile option in global profileOptions.
        * Return: (Chart) Chart for current output variable including all points plotted available in the output.
        */
        public Chart CreateProfileChart(int outputIndex, int profileIndex)
        {
            Chart chart = new Chart();
            ChartArea chartArea = new ChartArea();

            chart.Location = new System.Drawing.Point(3, 6);
            chart.Size = new System.Drawing.Size(1144, 440);
            chart.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Chart_MouseClick);

            chartArea.AxisX.Title = profileOptions[profileIndex].method.ToString() + " (" + GetProperXAxisUnits(profileOptions[profileIndex].method) + ")";
            chartArea.AxisY.Title = "Logarithmic Base 10 Representation\n" + GetProperYAxisUnits(outputIndex);
            chartArea.CursorX.IsUserEnabled = true;
            chartArea.CursorX.IsUserSelectionEnabled = true;
            chartArea.AxisY.IsStartedFromZero = false;
            chartArea.AxisY.LogarithmBase = 10;

            Series series1 = new Series
            {
                Name = outputVariables[outputIndex],
                Color = System.Drawing.Color.Blue,
                ChartType = SeriesChartType.Line,
                IsValueShownAsLabel = true,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 9,
                BorderWidth = 3
            };

            series1.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Partial;
            series1.SmartLabelStyle.IsMarkerOverlappingAllowed = false;
            series1.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top;
            series1.SmartLabelStyle.MinMovingDistance = 10.0;
            series1.LabelFormat = "{0:#.#####E+0}";


            for (int i = 0; i < testData[profileIndex].Length; i++)
            {
                int stepValue = (int)profileOptions[profileIndex].startValue + (int)(profileOptions[profileIndex].stepValue * i);

                double yValue = 0.0;
                if (outputIndex < 9)
                {
                    yValue = GetPointInLogForm(testData[profileIndex][i].Output.Densities[outputIndex]);
                    series1.Points.AddXY(stepValue, yValue);
                }
                else
                {
                    yValue = GetPointInLogForm(testData[profileIndex][i].Output.Temperature[outputIndex - 9]);
                    series1.Points.AddXY(stepValue, yValue);
                }
            }

            chart.Series.Clear();
            chart.ChartAreas.Add(chartArea);
            chart.Series.Add(series1);

            return chart;
        }

        /*
        * Name: CreateProfileGroupBox
        * Purpose: Creates a groupbox with labels to display the profile options selected and being displayed in charts.
        * Input: (ProfileOption) profileOption = Information about the ProfileOption to display inside the groupbox (start, stop, step)
        * Return: (GroupBox) Box containing 3 labels to display start, stop, and step values
        */
        public GroupBox CreateProfileGroupBox(ProfileOption profileOption)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Location = new System.Drawing.Point(7, 3);
            groupBox.Name = "profileOptionsGroupBox";
            groupBox.Size = new System.Drawing.Size(167, 65);
            groupBox.TabIndex = 1;
            groupBox.TabStop = false;
            groupBox.Text = "Profile Options";

            //Start label
            Label startLabel = CreateProfileLabel("Start: " + profileOption.startValue);
            groupBox.Controls.Add(startLabel);
            startLabel.Location = new System.Drawing.Point(6, 16);

            //Stop label
            Label stopLabel = CreateProfileLabel("Stop: " + profileOption.stopValue);
            groupBox.Controls.Add(stopLabel);
            stopLabel.Location = new System.Drawing.Point(6, 29);

            //Start label
            Label stepLabel = CreateProfileLabel("Step: " + profileOption.stepValue);
            groupBox.Controls.Add(stepLabel);
            stepLabel.Location = new System.Drawing.Point(6, 42);

            return groupBox;
        }

        /*
        * Name: CreateSelectionBox
        * Purpose: Creates a groupbox with labels to display the closest point to mouse click event
        * Return: (GroupBox) Box containing 2 labels to display x, and y values of closest point
        */
        public GroupBox CreateSelectionBox()
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Location = new System.Drawing.Point(442, 13);
            groupBox.Name = "selectionGroupBox";
            groupBox.Size = new System.Drawing.Size(270, 55);
            groupBox.Text = "Closest Point";

            //X label
            Label xLabel = CreateProfileLabel("X: ");
            xLabel.Name = "xLabel";
            groupBox.Controls.Add(xLabel);
            xLabel.Location = new System.Drawing.Point(6, 16);

            //X label
            Label yLabel = CreateProfileLabel("Y: ");
            yLabel.Name = "yLabel";
            groupBox.Controls.Add(yLabel);
            yLabel.Location = new System.Drawing.Point(6, 29);

            return groupBox;
        }

        /*
        * Name: CreateProfileLabel
        * Purpose: Creates default label with the same properties for both CreateProfileGroupBox, and CreateSelectionBox
        * Input: (string) text = Text to display on the new label
        * Return: (Label) Label with desired size and input text
        */
        public Label CreateProfileLabel(string text)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Size = new System.Drawing.Size(32, 13);
            label.Text = text;
            return label;
        }
        #endregion

        #region Utilities
        /*
        * Name: GetPointInLogForm
        * Purpose: Return the log base 10 representation of the input pointValue
        * Input: (double) pointValue = Current point value to apply log base 10
        * Return: (double) value of the point with log base 10 scale applied
        * Comments: Invalid point values will return 0.0
        */
        public double GetPointInLogForm(double pointValue)
        {
            if (pointValue <= 0.0)
            {
                return 0.0;
            }
            return Math.Log(pointValue, 10);
        }

        /*
        * Name: ReverseLogForm
        * Purpose: Return the normal value from a log base 10 representation
        * Input: (double) pointValue = Current point value which has already been reduced using log function
        * Return: (double) value of the point with log base 10 removed
        * Comments: Invalid point values will return 0.0
        */
        public double ReverseLogForm(double pointValue)
        {
            if (pointValue == 0.0)
            {
                return 0.0;
            }

            return Math.Pow(10, pointValue);
        }

        /*
        * Name: GetProperYAxisUnits
        * Purpose: Return the correct units from given output variables index
        * Input: (int) index = Index of the current element in outputVariables
        * Return: (string) correct units for the selected element
        */
        public string GetProperYAxisUnits(int index)
        {
            if (index == 5)
            {
                return "MASS DENSITY (GM/CM3)";
            }

            if (index == 9 || index == 10)
            {
                return "TEMPERATURE (K)";
            }

            return "DENSITY (CM3)";
        }

        /*
        * Name: GetProperXAxisUnits
        * Purpose: Get correct units for the given profile method
        * Input: (ProfileMethod) profileMethod = method used for determining the desired units
        * Return: (string) correct units for the selected profile method
        */
        public string GetProperXAxisUnits(ProfileMethod profileMethod)
        {
            switch(profileMethod)
            {
                case ProfileMethod.ALTITUDE:
                    return "KMs";

                case ProfileMethod.LATITUDE:
                case ProfileMethod.LONGITUDE:
                    return "DEGs";

                case ProfileMethod.MONTH:
                    return "MONTHs";

                case ProfileMethod.DAY_OF_MONTH:
                case ProfileMethod.DAY_OF_YEAR:
                    return "DAYs";

                case ProfileMethod.HOUR_OF_DAY:
                    return "HOURs";

                default:
                    return "";
            }
        }

        /*
        * Name: GetXUnitsFromParent
        * Purpose: Get correct units based on the TabPage text
        * Input: (string) parentText = text property of the controls parent TabPage.
        * Return: (string) correct units for the input text
        */
        public string GetXUnitsFromParent(string parentText)
        {
            switch(parentText)
            {
                case "ALTITUDE":
                    return " KMs";

                case "LATITUDE":
                case "LONGITUDE":
                    return " DEGs";

                case "MONTH":
                    return " months";

                case "DAY_OF_MONTH":
                case "DAY_OF_YEAR":
                    return " days";

                case "HOUR_OF_DAY":
                    return " hours";

                default:
                    return "";
            }
        }

        /*
        * Name: GetYUnitsFromParent
        * Purpose: Get correct units based on the TabPage text
        * Input: (string) parentText = text property of the controls parent TabPage.
        * Return: (string) correct units for the input text
        */
        public string GetYUnitsFromParent(string parentText)
        {
            int index = Array.IndexOf(outputVariables, parentText);
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 7:
                case 6:
                case 8:
                    return " CM3";

                case 5:
                    return " GM/CM3";

                case 9:
                case 10:
                    return " K";

                default:
                    return "";
            }
        }
        #endregion
    }
}

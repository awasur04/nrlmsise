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
    internal partial class GraphResults : Form
    {

        #region Global Properties
        Input initialInput;
        Test[][] testData;
        ProfileOption[] profileOptions;

        string[] outputVariables = new string[] { "HELIUM (HE)", "ATOMIC OXYGEN (O)", "NITROGEN (N2)", "OXYGEN (O2)", "ARGON (AR)", "TOTAL MASS DENSITY (GM/CM3)", "HYDROGEN (H)", "NITROGEN (N)", "ANON. O. DENSITY", "EXOSPHERIC TEMP.", "TEMP. AT ALT." };
        #endregion

        #region Constructor
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
        public void DisplayInputParameters()
        {
            string inputData = JsonConvert.SerializeObject(this.initialInput, Formatting.Indented);
            inputParametersLabel.Text = inputData;
        }

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
        public double GetPointInLogForm(double pointValue)
        {
            if (pointValue <= 0.0)
            {
                return 0.0;
            }
            return Math.Log(pointValue, 10);
        }

        public double ReverseLogForm(double pointValue)
        {
            if (pointValue == 0.0)
            {
                return 0.0;
            }

            return Math.Pow(10, pointValue);
        }

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

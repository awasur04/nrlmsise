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
        //When running multiple tests only first graph has correct data
        //Maybe make ui bigger for easier reading of graph data
        //Figure out a way to handle one extreme value when rest are normal

        Input initialInput;
        Test[][] testData;
        ProfileOption[] profileOptions;

        string[] outputVariables = new string[] { "HELIUM (HE)", "OXYGEN (O)", "DINITROGEN (N2)", "DIOXYGEN (O2)", "ARGON (AR)", "TOTAL MASS DENSITY (GM/CM3)", "HYDROGEN (H)", "NITROGEN (N)", "ANON. O. DENSITY", "EXOSPHERIC TEMP.", "TEMP. AT ALT." };
        public GraphResults(Input initialInput, Test[][] testData, ProfileOption[] profileOptions)
        {
            this.initialInput = initialInput;
            this.testData = testData;
            this.profileOptions = profileOptions;

            InitializeComponent();
            DisplayInputParameters();
            CreateProfileTabs();
        }


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
                tp.Controls.Add(CreateProfileTabControls(profileOptions[i], i));
                tp.Text = profileOptions[i].method.ToString();
                tp.Location = new System.Drawing.Point(4, 22);
                tp.Size = new System.Drawing.Size(863, 558);
                tp.UseVisualStyleBackColor = true;
                tabControl1.TabPages.Add(tp);
            }
        }

        public TabControl CreateProfileTabControls(ProfileOption profileOption, int profileIndex)
        {
            TabControl profileTabControl = new TabControl();
            profileTabControl.Location = new System.Drawing.Point(7, 74);
            profileTabControl.SelectedIndex = 0;
            profileTabControl.Size = new System.Drawing.Size(852, 478);

            for (int i = 0; i < outputVariables.Length; i++)
            {
                TabPage profileStepPage = new TabPage(outputVariables[i]);
                profileStepPage.Location = new System.Drawing.Point(7, 74);
                profileStepPage.Size = new System.Drawing.Size(852, 478);

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
            chart.Size = new System.Drawing.Size(835, 440);

            chartArea.AxisX.Title = "Profile Step Value";
            chartArea.AxisY.Title = GetProperUnits(outputIndex);
            chartArea.AxisY.LabelStyle.Format = "{0:#.#####E+0}";


            Series series1 = new Series
            {
                Name = outputVariables[outputIndex],
                Color = System.Drawing.Color.Blue,
                ChartType = SeriesChartType.Line,
                IsVisibleInLegend = true,
                IsValueShownAsLabel = true,
                MarkerStyle = MarkerStyle.Diamond
            };
            series1.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Partial;
            series1.SmartLabelStyle.IsMarkerOverlappingAllowed = false;
            series1.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Top;
            series1.SmartLabelStyle.MinMovingDistance = 10.0;
            series1.LabelFormat = "{0:#.#####E+0}";

            for (int i = 0; i < testData[profileIndex].Length; i++)
            {
                int stepValue = (int)profileOptions[profileIndex].startValue + (int)(profileOptions[profileIndex].stepValue * i);
                if (outputIndex < 9)
                {
                    series1.Points.AddXY(stepValue, testData[profileIndex][i].Output.Densities[outputIndex]);
                }
                else
                {
                    series1.Points.AddXY(stepValue, testData[profileIndex][i].Output.Temperature[outputIndex - 9]);
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

        public Label CreateProfileLabel(string text)
        {
            Label label = new Label();
            label.AutoSize = true;
            label.Size = new System.Drawing.Size(32, 13);
            label.Text = text;
            return label;
        }

        public string GetProperUnits(int index)
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
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThirdYearLP_IP_Solver.Algorithms;
using ThirdYearLP_IP_Solver.HelperClasses;
using ThirdYearLP_IP_Solver.Model;

using ThirdYearLP_IP_Solver.Assets;




namespace ThirdYearLP_IP_Solver
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        public int selectedAlgorithm;

        public MainForm()
        {
            InitializeComponent();


        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        

            string relativePath = @"..\..\Assets\LPModel1.txt";
            string filePath = Path.Combine(Application.StartupPath, relativePath);

            string musicRelPath = @"..\..\Assets\Megalovania.mp3";
            string musicFilePath = Path.Combine(Application.StartupPath, musicRelPath);

            try
            {
                Slay slay = new Slay(musicFilePath);
             

                InputParser parser = new InputParser(filePath);

                string lpTableString = parser.GetLPTableAsString();
                richTextBox1.Text = lpTableString;
                LPProblem lpProblem = parser.GetLPProblemData();

                // populate algorithms in ComboBox
                cmbxAlgorithmn.Items.Clear();
                cmbxAlgorithmn.Items.Add("Primal Simplex");
                cmbxAlgorithmn.Items.Add("Dual Simplex");
                cmbxAlgorithmn.Items.Add("Branch&Bound");
                cmbxAlgorithmn.Items.Add("Branch&Bound (Knapsack)");
                cmbxAlgorithmn.Items.Add("Cutting Plane");

                cmbxAlgorithmn.SelectedIndex = 0; // default selection
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading LP model: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            string relativePath = @"..\..\Assets\LPModel1.txt";
            string filePath = Path.Combine(Application.StartupPath, relativePath);

            InputParser parser = new InputParser(filePath);

            LPProblem lpProblem = parser.GetLPProblemData();
            // Build and bind DataTable
            resultsGridView.DataSource = BuildDataTable(lpProblem);
        }


        private void defaultBtn_Click(object sender, EventArgs e)
        {
         

        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
         
        }


        private DataTable BuildDataTable(LPProblem lpProblem)
        {
            DataTable table = new DataTable();

            int variableCount = lpProblem.VariableCount;
            int slackCount = lpProblem.SlackVariableCount;

            // Columns: BasicVar, x1..xn, s1..sk, RHS
            table.Columns.Add("Basic Var");
            for (int i = 1; i <= variableCount; i++)
                table.Columns.Add($"x{i}", typeof(double));

            for (int i = 1; i <= slackCount; i++)
                table.Columns.Add($"s{i}", typeof(double));

            table.Columns.Add("RHS", typeof(double));

            // Add constraint rows
            int slackIndex = 1;
            foreach (var constraint in lpProblem.Constraints)
            {
                DataRow row = table.NewRow();

                // Basic variable
                if (constraint.InequalityType == "<=")
                {
                    row["Basic Var"] = $"s{slackIndex}";
                    slackIndex++;
                }
                else
                {
                    row["Basic Var"] = "–"; // non-basic (>=, =) will need artificial vars in two-phase
                }

                // Decision variables
                for (int i = 0; i < variableCount; i++)
                {
                    row[$"x{i + 1}"] = (i < constraint.Coefficients.Length) ? constraint.Coefficients[i] : 0;
                }

                // Slack variables
                for (int i = 0; i < slackCount; i++)
                {
                    row[$"s{i + 1}"] = (constraint.InequalityType == "<=" && i == slackIndex - 2) ? 1 : 0;
                }

                // RHS
                row["RHS"] = constraint.RHS;

                table.Rows.Add(row);
            }

            // Objective row
            DataRow zRow = table.NewRow();
            zRow["Basic Var"] = "Z";

            for (int i = 0; i < variableCount; i++)
            {
                zRow[$"x{i + 1}"] = -lpProblem.ObjectiveCoefficients[i];
            }

            for (int i = 0; i < slackCount; i++)
            {
                zRow[$"s{i + 1}"] = 0;
            }

            zRow["RHS"] = 0;
            table.Rows.Add(zRow);

            return table;
        }
        /*
         * 
Primal Simplex
Dual Simplex
Branch&Bound
Branch&Bound (Knapsack)
Cutting Plane
         * */

        private void goBtn_Click(object sender, EventArgs e)
        {
            if (cmbxAlgorithmn.SelectedItem == null)
            {
                MessageBox.Show("Please select an algorithm first.", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedText = cmbxAlgorithmn.SelectedItem.ToString();

            try
            {
                switch (selectedText)
                {
                    case "Primal Simplex":
                        selectedAlgorithm = 0;
                        break;
                    case "Dual Simplex":
                        selectedAlgorithm = 1;
                        break;
                    case "Branch&Bound":
                        selectedAlgorithm = 2;
                        break;
                    case "Branch&Bound (Knapsack)":
                        selectedAlgorithm = 3;
                        break;
                    case "Cutting Plane":
                        selectedAlgorithm = 4;
                        break;
                    default:
                        selectedAlgorithm = 0;
                        break;
                }

                Console.WriteLine($"Selected Algorithm: {selectedText} (id={selectedAlgorithm})");
            }
            catch
            {
                Console.WriteLine("Couldn't get selected algorithm");
            }

            // TODO: Call algorithm class depending on `selectedAlgorithm`
            // Run algorithm
            if (selectedAlgorithm == 0) // Primal Simplex
            {
                string relativePath = @"..\..\Assets\LPModel1.txt";
                string filePath = Path.Combine(Application.StartupPath, relativePath);
                InputParser parser = new InputParser(filePath);

                LPProblem lpProblem = parser.GetLPProblemData();
                PrimalSimplex simplex = new PrimalSimplex(lpProblem);
                simplex.Solve();

                // Show the first iteration for now
                resultsGridView.DataSource = simplex.Iterations[0];
            }

        }

        private void btnExport_Click(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThirdYearLP_IP_Solver.HelperClasses;
using System.IO;



namespace ThirdYearLP_IP_Solver
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

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
            try
            {
                InputParser parser = new InputParser(filePath);

                // Option 1: Get formatted string representation
                string lpTableString = parser.GetLPTableAsString();

                // Display in a TextBox, RichTextBox, or console
               
                richTextBox1.Text = lpTableString;
                // Option 2: Get structured data for calculations
                LPProblem lpProblem = parser.GetLPProblemData();

              
                ProcessLPProblem(lpProblem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading LP model: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void ProcessLPProblem(LPProblem problem)
        {
          
            Console.WriteLine($"Objective Type: {problem.ObjectiveType}");
            Console.WriteLine($"Number of variables: {problem.ObjectiveCoefficients.Length}");
            Console.WriteLine($"Constraint type: {problem.ConstraintType}");
            Console.WriteLine($"RHS value: {problem.RHS}");

            
        }
    }
}

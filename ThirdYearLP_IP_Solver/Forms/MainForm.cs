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
            InputParser parser = new InputParser(filePath);
           
            richTextBox1.Text = parser.GetLPTableAsString();

        }
    }
}

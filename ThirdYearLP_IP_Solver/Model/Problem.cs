using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class Problem
    {
        public double[][] consMatrx;    // Constraint coefficient matrix
        public string[] signs;          // Array of constraint signs
        public double[] freeVars;       // Right-hand side values
        public double[] funcVars;       // Objective function coefficients
        public double c;                // Objective function constant
        public bool isExtrMax;          // Maximization flag

        public Problem(double[][] constraintMatrix, string[] signs, double[] freeVariables,
                      double[] functionVariables, double c, bool isExtrMax)
        {
            this.consMatrx = constraintMatrix;
            this.signs = signs;
            this.freeVars = freeVariables;
            this.funcVars = functionVariables;
            this.c = c;
            this.isExtrMax = isExtrMax;
        }
    }
}

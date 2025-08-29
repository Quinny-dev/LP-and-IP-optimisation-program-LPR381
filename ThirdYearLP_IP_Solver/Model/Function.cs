using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class Function
    {
        public double[] variables;  // Coefficients of objective function variables
        public double c;           // Constant term
        public bool isExtrMax;     // True for maximization, false for minimization

        public Function(double[] variables, double c, bool isExtrMax)
        {
            this.variables = variables;
            this.c = c;
            this.isExtrMax = isExtrMax;
        }
    }
}

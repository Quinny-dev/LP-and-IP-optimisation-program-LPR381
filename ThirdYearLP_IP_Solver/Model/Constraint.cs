using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class Constraint
    {
        public double[] variables;  // Coefficients of constraint variables
        public double b;           // Right-hand side value
        public string sign;        // "<=", ">=", or "="

        public Constraint(double[] variables, double b, string sign)
        {
            this.variables = variables;
            this.b = b;
            this.sign = sign;
        }
    }
}

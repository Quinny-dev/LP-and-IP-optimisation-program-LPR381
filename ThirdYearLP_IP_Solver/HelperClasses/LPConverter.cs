using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.Model;


namespace ThirdYearLP_IP_Solver.HelperClasses
{
    public static class LPConverter
    {
        public static (Function, Model.Constraint[]) Convert(LPProblem lp)
        {
            // Objective
            var function = new Function(lp.ObjectiveCoefficients, 0, lp.ObjectiveType == "max");

            // Constraints
            var constraints = lp.Constraints.Select(c => new Model.Constraint(
                c.Coefficients,
                c.RHS,
                c.InequalityType
            )).ToArray();

            return (function, constraints);
        }
    }
}

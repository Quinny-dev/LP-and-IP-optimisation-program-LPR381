using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class ProblemsService
    {
        public static ProblemsService shared = new ProblemsService();
        private Problem[] problems;
        private int currentIndex = 0;

        private ProblemsService()
        {
            InitializeProblems();
        }

        private void InitializeProblems()
        {
            problems = new Problem[]
            {
                // Example 1: Basic maximization problem
                // max 3x1 + 2x2
                // subject to: x1 + 2x2 <= 10, 2x1 + x2 <= 8
                new Problem(
                    new double[][] {
                        new double[] { 1, 2 },
                        new double[] { 2, 1 }
                    },
                    new string[] { "<=", "<=" },
                    new double[] { 10, 8 },
                    new double[] { 3, 2 },
                    0,
                    true
                ),

                // Example 2: Mixed constraints
                // max x1 + x2
                // subject to: x1 + x2 >= 2, x1 - x2 <= 1
                new Problem(
                    new double[][] {
                        new double[] { 1, 1 },
                        new double[] { 1, -1 }
                    },
                    new string[] { ">=", "<=" },
                    new double[] { 2, 1 },
                    new double[] { 1, 1 },
                    0,
                    true
                ),

                // Example 3: Minimization problem
                // min 2x1 + 3x2
                // subject to: x1 + x2 >= 4, 2x1 + x2 >= 6
                new Problem(
                    new double[][] {
                        new double[] { 1, 1 },
                        new double[] { 2, 1 }
                    },
                    new string[] { ">=", ">=" },
                    new double[] { 4, 6 },
                    new double[] { 2, 3 },
                    0,
                    false
                )
            };
        }

        public Problem GetNext()
        {
            Problem problem = problems[currentIndex];
            currentIndex = (currentIndex + 1) % problems.Length;
            return problem;
        }
    }
}
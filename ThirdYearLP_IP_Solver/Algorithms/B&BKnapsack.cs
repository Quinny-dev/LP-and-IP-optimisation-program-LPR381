using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    internal class B_BKnapsack
    {
            private LPProblem lpProblem;
            public bool[] Solution { get; private set; }
            public double MaxProfit { get; private set; }

            public B_BKnapsack(LPProblem problem)
            {
                lpProblem = problem;
                Solution = new bool[lpProblem.VariableCount];
            }

            public void Solve()
            {
                int n = lpProblem.VariableCount;
                double[] weights = lpProblem.Constraints[0].Coefficients;
                double[] profits = lpProblem.ObjectiveCoefficients;
                double capacity = lpProblem.Constraints[0].RHS;

                bool[] taken = new bool[n];
                bool[] bestTaken = new bool[n];
                double maxProfit = 0;

                void BB(int index, double currentWeight, double currentProfit)
                {
                    if (currentWeight > capacity) return;

                    if (index == n)
                    {
                        if (currentProfit > maxProfit)
                        {
                            maxProfit = currentProfit;
                            Array.Copy(taken, bestTaken, n);
                        }
                        return;
                    }

                    // Take current item
                    taken[index] = true;
                    BB(index + 1, currentWeight + weights[index], currentProfit + profits[index]);

                    // Do not take current item
                    taken[index] = false;
                    BB(index + 1, currentWeight, currentProfit);
                }

                BB(0, 0, 0);

                Solution = bestTaken;
                MaxProfit = maxProfit;
            }
    }
}


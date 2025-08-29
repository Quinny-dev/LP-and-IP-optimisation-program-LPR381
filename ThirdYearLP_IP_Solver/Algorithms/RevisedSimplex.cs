using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    internal class RevisedSimplex
    {
        private LPProblem lpProblem;

        public List<double[]> Iterations { get; private set; } = new List<double[]>();
        public double[] Solution { get; private set; }
        public double MaxValue { get; private set; }

        public RevisedSimplex(LPProblem problem)
        {
            lpProblem = problem;
        }

        public void Solve()
        {
            int m = lpProblem.Constraints.Count;
            int n = lpProblem.VariableCount;

            double[,] A = new double[m, n + m];
            double[] b = new double[m];
            double[] c = new double[n + m];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                    A[i, j] = lpProblem.Constraints[i].Coefficients[j];

                if (lpProblem.Constraints[i].RequiresSlackVariable)
                    A[i, n + i] = 1;

                b[i] = lpProblem.Constraints[i].RHS;
            }

            for (int j = 0; j < n; j++)
                c[j] = lpProblem.ObjectiveCoefficients[j];

            int[] basis = new int[m];
            for (int i = 0; i < m; i++)
                basis[i] = n + i;

            while (true)
            {
                double[,] B = new double[m, m];
                for (int i = 0; i < m; i++)
                    for (int j = 0; j < m; j++)
                        B[i, j] = A[i, basis[j]];

                double[,] Binv = InvertMatrix(B);

                double[] xB = Multiply(Binv, b);

                double[] cB = new double[m];
                for (int i = 0; i < m; i++)
                    cB[i] = c[basis[i]];

                double[] piT = Multiply(cB, Binv);

                double[] reducedCosts = new double[n + m];
                for (int j = 0; j < n + m; j++)
                {
                    double[] Aj = GetColumn(A, j);
                    reducedCosts[j] = c[j] - DotProduct(piT, Aj);
                }

                int entering = -1;
                double maxCost = 0;
                for (int j = 0; j < n + m; j++)
                {
                    if (!Array.Exists(basis, x => x == j) && reducedCosts[j] > maxCost)
                    {
                        maxCost = reducedCosts[j];
                        entering = j;
                    }
                }

                if (entering == -1)
                {
                    Solution = new double[n + m];
                    for (int i = 0; i < m; i++)
                        Solution[basis[i]] = xB[i];

                    MaxValue = DotProduct(c, Solution);
                    Iterations.Add((double[])Solution.Clone());
                    break;
                }

                double[] d = Multiply(Binv, GetColumn(A, entering));

                double minRatio = double.MaxValue;
                int leavingIndex = -1;
                for (int i = 0; i < m; i++)
                {
                    if (d[i] > 1e-8)
                    {
                        double ratio = xB[i] / d[i];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            leavingIndex = i;
                        }
                    }
                }

                if (leavingIndex == -1)
                    throw new Exception("Problem is unbounded.");

                basis[leavingIndex] = entering;
                Iterations.Add((double[])Solution.Clone());
            }
        }

        #region Helpers
        private double DotProduct(double[] a, double[] b)
        {
            double sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += a[i] * b[i];
            return sum;
        }

        private double[] Multiply(double[,] mat, double[] vec)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            double[] result = new double[rows];
            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                    sum += mat[i, j] * vec[j];
                result[i] = sum;
            }
            return result;
        }

        private double[] Multiply(double[] vec, double[,] mat)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            double[] result = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                double sum = 0;
                for (int i = 0; i < rows; i++)
                    sum += vec[i] * mat[i, j];
                result[j] = sum;
            }
            return result;
        }

        private double[,] InvertMatrix(double[,] a)
        {
            int n = a.GetLength(0);
            double[,] result = new double[n, n];
            double[,] copy = (double[,])a.Clone();

            for (int i = 0; i < n; i++)
                result[i, i] = 1;

            for (int i = 0; i < n; i++)
            {
                double pivot = copy[i, i];
                if (Math.Abs(pivot) < 1e-10)
                    throw new Exception("Matrix is singular.");

                for (int j = 0; j < n; j++)
                {
                    copy[i, j] /= pivot;
                    result[i, j] /= pivot;
                }

                for (int k = 0; k < n; k++)
                {
                    if (k == i) continue;
                    double factor = copy[k, i];
                    for (int j = 0; j < n; j++)
                    {
                        copy[k, j] -= factor * copy[i, j];
                        result[k, j] -= factor * result[i, j];
                    }
                }
            }
            return result;
        }

        private double[] GetColumn(double[,] mat, int colIndex)
        {
            int rows = mat.GetLength(0);
            double[] col = new double[rows];
            for (int i = 0; i < rows; i++)
                col[i] = mat[i, colIndex];
            return col;
        }
        #endregion
    }

}


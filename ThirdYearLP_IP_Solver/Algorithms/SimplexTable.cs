using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{

    public class SimplexTable
    {
        public string[] BasicVariables { get; set; }
        public string[] AllVariables { get; set; }  // x1, x2, s1, s2, etc
        public double[,] TableData { get; set; }    // The actual table values
        public double[] RHS { get; set; }           // Right-hand side values
        public double[] ObjectiveRow { get; set; }  // Z row 
        public int Iteration { get; set; }
        public bool IsOptimal { get; set; }
        public string OptimalityStatus { get; set; }

        public SimplexTable(int numConstraints, int numVariables)
        {
            BasicVariables = new string[numConstraints];
            AllVariables = new string[numVariables];
            TableData = new double[numConstraints, numVariables];
            RHS = new double[numConstraints];
            ObjectiveRow = new double[numVariables];
            Iteration = 0;
            IsOptimal = false;
            OptimalityStatus = "Not Optimal";
        }

        // Create initial table from updated LPProblem
        public static SimplexTable CreateFromLPProblem(LPProblem problem)
        {
            int numDecisionVars = problem.VariableCount;
            int numConstraints = problem.ConstraintCount;
            int numSlackVars = problem.SlackVariableCount;
            int totalVars = numDecisionVars + numSlackVars;

            SimplexTable table = new SimplexTable(numConstraints, totalVars);

            // Set up variable names
            for (int i = 0; i < numDecisionVars; i++)
            {
                table.AllVariables[i] = $"x{i + 1}";
            }

            // Add slack variable names
            int slackIndex = 0;
            for (int i = 0; i < numSlackVars; i++)
            {
                table.AllVariables[numDecisionVars + i] = $"s{i + 1}";
            }

            // Set up constraint rows
            slackIndex = 0;
            for (int constraintIdx = 0; constraintIdx < numConstraints; constraintIdx++)
            {
                var constraint = problem.Constraints[constraintIdx];

                // Set basic variable name
                if (constraint.RequiresSlackVariable)
                {
                    table.BasicVariables[constraintIdx] = $"s{slackIndex + 1}";
                }
                else
                {
                    table.BasicVariables[constraintIdx] = $"a{constraintIdx + 1}"; // Artificial variable for = or >=
                }

                // Set decision variable coefficients
                for (int j = 0; j < numDecisionVars; j++)
                {
                    if (j < constraint.Coefficients.Length)
                    {
                        table.TableData[constraintIdx, j] = constraint.Coefficients[j];
                    }
                    else
                    {
                        table.TableData[constraintIdx, j] = 0; // Pad with zeros if needed
                    }
                }

                // Set slack variable coefficients
                for (int j = 0; j < numSlackVars; j++)
                {
                    if (constraint.RequiresSlackVariable && j == slackIndex)
                    {
                        table.TableData[constraintIdx, numDecisionVars + j] = 1;
                    }
                    else
                    {
                        table.TableData[constraintIdx, numDecisionVars + j] = 0;
                    }
                }

                // Set RHS
                table.RHS[constraintIdx] = constraint.RHS;

                if (constraint.RequiresSlackVariable)
                {
                    slackIndex++;
                }
            }

            // Set up objective row for maximization
            for (int j = 0; j < numDecisionVars; j++)
            {
                if (j < problem.ObjectiveCoefficients.Length)
                {
                    table.ObjectiveRow[j] = problem.ObjectiveType.ToLower() == "max"
                        ? -problem.ObjectiveCoefficients[j]
                        : problem.ObjectiveCoefficients[j];
                }
                else
                {
                    table.ObjectiveRow[j] = 0;
                }
            }

            // Slack variables have 0 coefficient in objective
            for (int j = numDecisionVars; j < totalVars; j++)
            {
                table.ObjectiveRow[j] = 0;
            }

            return table;
        }

        // Convert to DataTable for DataGridView 
        public DataTable ToDataTable()
        {
            DataTable dt = new DataTable();

            // Add columns
            dt.Columns.Add("Basic Var", typeof(string));
            foreach (string var in AllVariables)
            {
                dt.Columns.Add(var, typeof(double));
            }
            dt.Columns.Add("RHS", typeof(double));

            // Add constraint rows
            for (int i = 0; i < BasicVariables.Length; i++)
            {
                DataRow row = dt.NewRow();
                row["Basic Var"] = BasicVariables[i];

                for (int j = 0; j < AllVariables.Length; j++)
                {
                    row[AllVariables[j]] = Math.Round(TableData[i, j], 4); // Round for display
                }

                row["RHS"] = Math.Round(RHS[i], 4);
                dt.Rows.Add(row);
            }

            // Add objective row
            DataRow objRow = dt.NewRow();
            objRow["Basic Var"] = "Z";

            for (int j = 0; j < AllVariables.Length; j++)
            {
                objRow[AllVariables[j]] = Math.Round(ObjectiveRow[j], 4);
            }

            objRow["RHS"] = 0;
            dt.Rows.Add(objRow);

            return dt;
        }

        // Get current solution values
        public Dictionary<string, double> GetSolutionValues()
        {
            var solution = new Dictionary<string, double>();

            // Initialize all variables to 0
            foreach (string var in AllVariables)
            {
                solution[var] = 0;
            }

            // Set basic variables to their RHS values
            for (int i = 0; i < BasicVariables.Length; i++)
            {
                solution[BasicVariables[i]] = RHS[i];
            }

            return solution;
        }

        // Get objective function value
        public double GetObjectiveValue()
        {
            var solution = GetSolutionValues();
            double objValue = 0;

            // This would need the original objective coefficients to calculate properly
            // For now, we can estimate from the current table state
            return objValue;
        }
    }

    // Class to handle simplex algorithm calculations
    public class SimplexSolver
    {
        public List<SimplexTable> Iterations { get; private set; }
        public SimplexTable CurrentTable { get; private set; }
        public LPProblem Problem { get; private set; }

        public SimplexSolver()
        {
            Iterations = new List<SimplexTable>();
        }

        public void Initialize(LPProblem problem)
        {
            Problem = problem;
            CurrentTable = SimplexTable.CreateFromLPProblem(problem);
            CurrentTable.Iteration = 0;
            Iterations.Add(CurrentTable);
        }

        public bool IsOptimal()
        {
            // For maximization: optimal when all objective row values >= 0
            // For minimization: optimal when all objective row values <= 0
            if (Problem.ObjectiveType.ToLower() == "max")
            {
                return CurrentTable.ObjectiveRow.All(x => x >= -1e-10); // Small tolerance for floating point
            }
            else
            {
                return CurrentTable.ObjectiveRow.All(x => x <= 1e-10);
            }
        }

        public (int pivotRow, int pivotCol) FindPivotElement()
        {
            int pivotCol = -1;

            // Find entering variable
            if (Problem.ObjectiveType.ToLower() == "max")
            {
                // Most negative value in objective row
                double mostNegative = -1e-10; // Use small tolerance
                for (int j = 0; j < CurrentTable.ObjectiveRow.Length; j++)
                {
                    if (CurrentTable.ObjectiveRow[j] < mostNegative)
                    {
                        mostNegative = CurrentTable.ObjectiveRow[j];
                        pivotCol = j;
                    }
                }
            }
            else
            {
                // Most positive value in objective row for minimization
                double mostPositive = 1e-10;
                for (int j = 0; j < CurrentTable.ObjectiveRow.Length; j++)
                {
                    if (CurrentTable.ObjectiveRow[j] > mostPositive)
                    {
                        mostPositive = CurrentTable.ObjectiveRow[j];
                        pivotCol = j;
                    }
                }
            }

            if (pivotCol == -1) return (-1, -1); // Already optimal

            // Find leaving variable using minimum ratio test
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 0; i < CurrentTable.RHS.Length; i++)
            {
                if (CurrentTable.TableData[i, pivotCol] > 1e-10) // Use tolerance
                {
                    double ratio = CurrentTable.RHS[i] / CurrentTable.TableData[i, pivotCol];
                    if (ratio >= 0 && ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            if (pivotRow == -1)
            {
                // Unbounded solution
                return (-1, -1);
            }

            return (pivotRow, pivotCol);
        }

        public bool PerformIteration()
        {
            if (IsOptimal())
            {
                CurrentTable.IsOptimal = true;
                CurrentTable.OptimalityStatus = "Optimal Solution Found";
                return false;
            }

            var (pivotRow, pivotCol) = FindPivotElement();

            if (pivotRow == -1 || pivotCol == -1)
            {
                CurrentTable.OptimalityStatus = "Unbounded or No Solution";
                return false;
            }

            // Create new table for next iteration
            SimplexTable newTable = PerformPivotOperation(pivotRow, pivotCol);
            newTable.Iteration = CurrentTable.Iteration + 1;

            CurrentTable = newTable;
            Iterations.Add(CurrentTable);

            return true;
        }

        private SimplexTable PerformPivotOperation(int pivotRow, int pivotCol)
        {
            // Create a copy of current table structure
            int numConstraints = CurrentTable.BasicVariables.Length;
            int numVars = CurrentTable.AllVariables.Length;

            SimplexTable newTable = new SimplexTable(numConstraints, numVars);
            Array.Copy(CurrentTable.BasicVariables, newTable.BasicVariables, numConstraints);
            Array.Copy(CurrentTable.AllVariables, newTable.AllVariables, numVars);

            // Update basic variable (entering variable replaces leaving variable)
            newTable.BasicVariables[pivotRow] = CurrentTable.AllVariables[pivotCol];

            double pivotElement = CurrentTable.TableData[pivotRow, pivotCol];

            // Check for zero pivot (shouldn't happen with proper pivot selection)
            if (Math.Abs(pivotElement) < 1e-10)
            {
                throw new InvalidOperationException("Pivot element is too close to zero");
            }

            // Update pivot row (divide by pivot element)
            for (int j = 0; j < numVars; j++)
            {
                newTable.TableData[pivotRow, j] = CurrentTable.TableData[pivotRow, j] / pivotElement;
            }
            newTable.RHS[pivotRow] = CurrentTable.RHS[pivotRow] / pivotElement;

            // Update other constraint rows using row operations
            for (int i = 0; i < numConstraints; i++)
            {
                if (i != pivotRow)
                {
                    double multiplier = CurrentTable.TableData[i, pivotCol];
                    for (int j = 0; j < numVars; j++)
                    {
                        newTable.TableData[i, j] = CurrentTable.TableData[i, j] -
                                                  multiplier * newTable.TableData[pivotRow, j];
                    }
                    newTable.RHS[i] = CurrentTable.RHS[i] - multiplier * newTable.RHS[pivotRow];
                }
            }

            // Update objective row
            double objMultiplier = CurrentTable.ObjectiveRow[pivotCol];
            for (int j = 0; j < numVars; j++)
            {
                newTable.ObjectiveRow[j] = CurrentTable.ObjectiveRow[j] -
                                          objMultiplier * newTable.TableData[pivotRow, j];
            }

            return newTable;
        }

        public void SolveToOptimal()
        {
            int maxIterations = 1000; // Prevent infinite loops
            int iterations = 0;

            while (PerformIteration() && iterations < maxIterations)
            {
                iterations++;
            }

            if (iterations >= maxIterations)
            {
                CurrentTable.OptimalityStatus = "Maximum iterations reached - possible cycling";
            }
        }

        // Get final solution in readable format
        public string GetSolutionSummary()
        {
            if (!CurrentTable.IsOptimal)
            {
                return $"Solution Status: {CurrentTable.OptimalityStatus}";
            }

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("=== OPTIMAL SOLUTION FOUND ===");
            summary.AppendLine($"Iterations: {CurrentTable.Iteration}");
            summary.AppendLine();

            var solution = CurrentTable.GetSolutionValues();

            // Show decision variables
            summary.AppendLine("Decision Variables:");
            for (int i = 0; i < Problem.VariableCount; i++)
            {
                string varName = $"x{i + 1}";
                summary.AppendLine($"  {varName} = {Math.Round(solution[varName], 4)}");
            }

            summary.AppendLine();
            summary.AppendLine("Basic Variables:");
            for (int i = 0; i < CurrentTable.BasicVariables.Length; i++)
            {
                summary.AppendLine($"  {CurrentTable.BasicVariables[i]} = {Math.Round(CurrentTable.RHS[i], 4)}");
            }

            return summary.ToString();
        }
    }
}

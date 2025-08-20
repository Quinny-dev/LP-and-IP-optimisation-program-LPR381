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
        public string[] AllVariables { get; set; }  // x1, x2, s1, s2, ect
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

        // Create initial table 
        public static SimplexTable CreateFromLPProblem(LPProblem problem)
        {
            int numVars = problem.ObjectiveCoefficients.Length;
            int numSlackVars = problem.HasSlackVariable ? 1 : 0;
            int totalVars = numVars + numSlackVars;

            SimplexTable table = new SimplexTable(1, totalVars);

            // Set up variable names
            for (int i = 0; i < numVars; i++)
            {
                table.AllVariables[i] = $"x{i + 1}";
            }

            if (problem.HasSlackVariable)
            {
                table.AllVariables[numVars] = "s1";
                table.BasicVariables[0] = "s1";
            }

            // Set up constraint row
            for (int i = 0; i < numVars; i++)
            {
                table.TableData[0, i] = problem.ConstraintCoefficients[i];
            }

            if (problem.HasSlackVariable)
            {
                table.TableData[0, numVars] = 1; 
            }

            table.RHS[0] = problem.RHS;

            // Set up objective row 
            for (int i = 0; i < numVars; i++)
            {
                table.ObjectiveRow[i] = problem.ObjectiveType.ToLower() == "max"
                    ? -problem.ObjectiveCoefficients[i]
                    : problem.ObjectiveCoefficients[i];
            }

            if (problem.HasSlackVariable)
            {
                table.ObjectiveRow[numVars] = 0; // Slack vars in objective
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
                    row[AllVariables[j]] = TableData[i, j];
                }

                row["RHS"] = RHS[i];
                dt.Rows.Add(row);
            }

            // Add objective row
            DataRow objRow = dt.NewRow();
            objRow["Basic Var"] = "Z";

            for (int j = 0; j < AllVariables.Length; j++)
            {
                objRow[AllVariables[j]] = ObjectiveRow[j];
            }

            objRow["RHS"] = 0; 
            dt.Rows.Add(objRow);

            return dt;
        }
    }
    //TODO: FIX CALCULATIONS 
    // Class to handle simplex algorithm calculations
    public class SimplexSolver
    {
        public List<SimplexTable> Iterations { get; private set; }
        public SimplexTable CurrentTable { get; private set; }

        public SimplexSolver()
        {
            Iterations = new List<SimplexTable>();
        }

        public void Initialize(LPProblem problem)
        {
            CurrentTable = SimplexTable.CreateFromLPProblem(problem);
            CurrentTable.Iteration = 0;
            Iterations.Add(CurrentTable);
        }

        public bool IsOptimal()
        {
            // Check if all objective row values are >= 0 (for max problem)
            return CurrentTable.ObjectiveRow.All(x => x >= 0);
        }

        public (int pivotRow, int pivotCol) FindPivotElement()
        {
            // Find entering variable (most negative in objective row for max problem)
            int pivotCol = -1;
            double mostNegative = 0;

            for (int j = 0; j < CurrentTable.ObjectiveRow.Length; j++)
            {
                if (CurrentTable.ObjectiveRow[j] < mostNegative)
                {
                    mostNegative = CurrentTable.ObjectiveRow[j];
                    pivotCol = j;
                }
            }

            if (pivotCol == -1) return (-1, -1); 

            // Find leaving variable using minimum ratio test
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 0; i < CurrentTable.RHS.Length; i++)
            {
                if (CurrentTable.TableData[i, pivotCol] > 0)
                {
                    double ratio = CurrentTable.RHS[i] / CurrentTable.TableData[i, pivotCol];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
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
            // Create a copy of current table
            int numConstraints = CurrentTable.BasicVariables.Length;
            int numVars = CurrentTable.AllVariables.Length;

            SimplexTable newTable = new SimplexTable(numConstraints, numVars);
            Array.Copy(CurrentTable.BasicVariables, newTable.BasicVariables, numConstraints);
            Array.Copy(CurrentTable.AllVariables, newTable.AllVariables, numVars);

            // Update basic variable
            newTable.BasicVariables[pivotRow] = CurrentTable.AllVariables[pivotCol];

            double pivotElement = CurrentTable.TableData[pivotRow, pivotCol];

            // Update pivot row
            for (int j = 0; j < numVars; j++)
            {
                newTable.TableData[pivotRow, j] = CurrentTable.TableData[pivotRow, j] / pivotElement;
            }
            newTable.RHS[pivotRow] = CurrentTable.RHS[pivotRow] / pivotElement;

            // Update other constraint rows
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
            while (PerformIteration())
            {
                // Continues until optimal or unbounded
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    public enum VariableSignType
    {
        Positive,
        Negative,
        Binary,
        Integer,
        Unrestricted
    }
    internal class Sensitivity_Analysis
    {
            private bool isConsoleOutput;
            private DualSimplex dual;
            private List<double> objFunc;
            private List<List<double>> constraints;

            // Store matrices for sensitivity analysis
            private List<List<double>> matrixB;
            private List<List<double>> matrixBNegOne;
            private List<List<double>> matrixCbv;
            private List<List<double>> matrixCbvNegOne;
            private List<int> basicVarSpots;
            private List<List<double>> firstTab;
            private List<List<double>> revisedTab;
            private List<string> headerStr;

            public Sensitivity_Analysis(bool isConsoleOutput = false)
            {
                this.isConsoleOutput = isConsoleOutput;
                dual = new DualSimplex();
                objFunc = new List<double> { 0.0, 0.0 };
                constraints = new List<List<double>> { new List<double> { 0.0, 0.0, 0.0, 0.0 } };

                // Initialize matrices
                matrixB = null;
                matrixBNegOne = null;
                matrixCbv = null;
                matrixCbvNegOne = null;
                basicVarSpots = new List<int>();
                firstTab = null;
                revisedTab = null;
                headerStr = new List<string>();
            }

            public List<List<double>> DoFormulationOperation(List<double> objFunc, List<List<double>> constraints)
            {
                int excessCount = 0;
                int slackCount = 0;

                for (int i = 0; i < constraints.Count; i++)
                {
                    if (constraints[i][constraints[i].Count - 1] == 1)
                        excessCount++;
                    else
                        slackCount++;
                }

                for (int i = 0; i < constraints.Count; i++)
                {
                    for (int j = 0; j < constraints[i].Count; j++)
                    {
                        if (constraints[i][constraints[i].Count - 1] == 1)
                        {
                            constraints[i][j] = -1 * constraints[i][j];
                        }
                    }
                }

                for (int i = 0; i < constraints.Count; i++)
                {
                    constraints[i].RemoveAt(constraints[i].Count - 1);
                }

                int tableSizeH = constraints.Count + 1;
                int tableSizeW = excessCount + slackCount + 1 + objFunc.Count;

                var opTable = new List<List<double>>();
                for (int i = 0; i < tableSizeH; i++)
                {
                    opTable.Add(new List<double>());
                    for (int j = 0; j < tableSizeW; j++)
                    {
                        opTable[i].Add(0);
                    }
                }

                for (int i = 0; i < objFunc.Count; i++)
                {
                    opTable[0][i] = -objFunc[i];
                }

                for (int i = 0; i < constraints.Count; i++)
                {
                    for (int j = 0; j < constraints[i].Count - 1; j++)
                    {
                        opTable[i + 1][j] = constraints[i][j];
                    }
                    opTable[i + 1][opTable[i + 1].Count - 1] = constraints[i][constraints[i].Count - 1];
                }

                // added the slack and excess 1s
                for (int i = 1; i < opTable.Count; i++)
                {
                    for (int j = objFunc.Count; j < opTable[i].Count - 1; j++)
                    {
                        opTable[i][i + objFunc.Count - 1] = 1;
                    }
                }

                return opTable;
            }

            public List<List<double>> MatTranspose(List<List<double>> matrix)
            {
                if (matrix.Count == 0) return new List<List<double>>();

                int rows = matrix.Count;
                int cols = matrix[0].Count;
                var result = new List<List<double>>();

                for (int j = 0; j < cols; j++)
                {
                    var newRow = new List<double>();
                    for (int i = 0; i < rows; i++)
                    {
                        newRow.Add(matrix[i][j]);
                    }
                    result.Add(newRow);
                }
                return result;
            }

            public List<List<double>> MatMultiply(List<List<double>> A, List<List<double>> B)
            {
                int rowsA = A.Count;
                int colsA = A[0].Count;
                int rowsB = B.Count;
                int colsB = B[0].Count;

                if (colsA != rowsB)
                    throw new ArgumentException("Incompatible dimensions for multiplication");

                var result = new List<List<double>>();
                for (int i = 0; i < rowsA; i++)
                {
                    var row = new List<double>();
                    for (int j = 0; j < colsB; j++)
                    {
                        double sum = 0;
                        for (int k = 0; k < colsA; k++)
                        {
                            sum += A[i][k] * B[k][j];
                        }
                        row.Add(sum);
                    }
                    result.Add(row);
                }
                return result;
            }

            public List<List<double>> MatIdentity(int n)
            {
                var result = new List<List<double>>();
                for (int i = 0; i < n; i++)
                {
                    var row = new List<double>();
                    for (int j = 0; j < n; j++)
                    {
                        row.Add(i == j ? 1.0 : 0.0);
                    }
                    result.Add(row);
                }
                return result;
            }

            public List<List<double>> MatInverse(List<List<double>> matrix)
            {
                int n = matrix.Count;
                int m = matrix[0].Count;
                if (n != m)
                    throw new ArgumentException("Matrix must be square for inversion");

                // Create augmented matrix
                var aug = new List<List<double>>();
                var identity = MatIdentity(n);

                for (int i = 0; i < n; i++)
                {
                    var row = new List<double>(matrix[i]);
                    row.AddRange(identity[i]);
                    aug.Add(row);
                }

                // Forward elimination
                for (int i = 0; i < n; i++)
                {
                    double pivot = aug[i][i];
                    if (Math.Abs(pivot) < 1e-10)
                    {
                        // Swap with a lower row
                        bool found = false;
                        for (int r = i + 1; r < n; r++)
                        {
                            if (Math.Abs(aug[r][i]) > 1e-10)
                            {
                                var temp = aug[i];
                                aug[i] = aug[r];
                                aug[r] = temp;
                                pivot = aug[i][i];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            throw new ArgumentException("Matrix is singular and cannot be inverted");
                    }

                    // Normalize pivot row
                    for (int j = 0; j < 2 * n; j++)
                    {
                        aug[i][j] /= pivot;
                    }

                    // Eliminate other rows
                    for (int r = 0; r < n; r++)
                    {
                        if (r != i)
                        {
                            double factor = aug[r][i];
                            for (int j = 0; j < 2 * n; j++)
                            {
                                aug[r][j] -= factor * aug[i][j];
                            }
                        }
                    }
                }

                // Extract inverse
                var result = new List<List<double>>();
                for (int i = 0; i < n; i++)
                {
                    var row = new List<double>();
                    for (int j = n; j < 2 * n; j++)
                    {
                        row.Add(aug[i][j]);
                    }
                    result.Add(row);
                }
                return result;
            }

            public List<List<double>> MatFromColVector(List<double> lst)
            {
                var result = new List<List<double>>();
                foreach (double x in lst)
                {
                    result.Add(new List<double> { x });
                }
                return result;
            }

            public void PrintMatrix(List<List<double>> M, string name = "Matrix")
            {
                Logger.WriteLine(name);
                var transposed = MatTranspose(M);
                foreach (var row in transposed)
                {
                    foreach (var val in row)
                    {
                        Logger.Write($"{val,15:F2} ");
                    }
                    Logger.WriteLine();
                }
                Logger.WriteLine();
            }

            public void DoPreliminaries(List<double> objFunc, List<List<double>> constraints, bool isMin)
            {
                // make temporary copies of objFunc and constraints
                var tObjFunc = objFunc.Select(x => x).ToList();
                var tConstraints = constraints.Select(row => row.Select(x => x).ToList()).ToList();

                var (tableaus, changingVars, optimalSolution, _, _, headerStr) = dual.DoDualSimplex(
                    tObjFunc, tConstraints, isMin);

                this.headerStr = headerStr.ToList();

                // get the spots of the basic variables
                var basicVarSpots = new List<int>();
                for (int k = 0; k < tableaus[tableaus.Count - 1][tableaus[tableaus.Count - 1].Count - 1].Count; k++)
                {
                    int columnIndex = k;
                    var tCVars = new List<double>();

                    for (int i = 0; i < tableaus[tableaus.Count - 1].Count; i++)
                    {
                        double columnValue = tableaus[tableaus.Count - 1][i][columnIndex];
                        tCVars.Add(columnValue);
                    }

                    if (Math.Abs(tCVars.Sum() - 1.0) < 1e-10)
                    {
                        basicVarSpots.Add(k);
                    }
                }

                this.basicVarSpots = basicVarSpots;

                // get the columns of the basic variables
                var basicVarCols = new List<List<double>>();
                for (int i = 0; i < tableaus[tableaus.Count - 1][tableaus[tableaus.Count - 1].Count - 1].Count; i++)
                {
                    var tLst = new List<double>();
                    if (basicVarSpots.Contains(i))
                    {
                        for (int j = 0; j < tableaus[tableaus.Count - 1].Count; j++)
                        {
                            tLst.Add(tableaus[tableaus.Count - 1][j][i]);
                        }
                        basicVarCols.Add(tLst);
                    }
                }

                // sort the cbv according the basic var positions
                var zippedCbv = basicVarCols.Zip(basicVarSpots, (col, spot) => new { Col = col, Spot = spot }).ToList();
                var sortedCbvZipped = zippedCbv.OrderBy(x => x.Col.Contains(1) ? x.Col.IndexOf(1) : x.Col.Count).ToList();

                var sortedBasicVars = sortedCbvZipped.Select(x => x.Col).ToList();
                basicVarSpots = sortedCbvZipped.Select(x => x.Spot).ToList();

                // populate matrixes
                var cbv = new List<double>();
                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    cbv.Add(-tableaus[0][0][basicVarSpots[i]]);
                }

                // if (isConsoleOutput)
                // {
                //     Logger.WriteLine(string.Join(", ", basicVarSpots));
                // }

                var matB = new List<List<double>>();
                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    var tLst = new List<double>();
                    for (int j = 1; j < tableaus[0].Count; j++)
                    {
                        tLst.Add(tableaus[0][j][basicVarSpots[i]]);
                    }
                    matB.Add(tLst);
                }

                var matrixCbv = MatFromColVector(cbv);
                var matrixB = matB;
                var matrixBNegOne = MatInverse(matrixB);
                var matrixCbvNegOne = MatMultiply(matrixBNegOne, matrixCbv);

                // Store matrices for sensitivity analysis
                this.matrixB = matrixB;
                this.matrixBNegOne = matrixBNegOne;
                this.matrixCbv = matrixCbv;
                this.matrixCbvNegOne = matrixCbvNegOne;

                if (isConsoleOutput)
                {
                    PrintMatrix(matrixCbv, "cbv");
                    PrintMatrix(matrixB, "B");
                    PrintMatrix(matrixBNegOne, "B^-1");
                    PrintMatrix(matrixCbvNegOne, "cbvB^-1");
                    Logger.WriteLine();
                }

                // work with the final tableau directly
                var firstTab = tableaus[0];
                this.firstTab = firstTab.Select(row => row.Select(x => x).ToList()).ToList();

                // get the z values of the new changing table (same size as optimal table)
                var changingZRow = new List<double>();
                for (int j = 0; j < firstTab[firstTab.Count - 1].Count - 1; j++)  // skip RHS column
                {
                    // column under variable j (skip row 0)
                    var tCol = new List<double>();
                    for (int i = 1; i < firstTab.Count; i++)
                    {
                        tCol.Add(firstTab[i][j]);
                    }
                    // make row vector and multiply with cbvB^-1
                    var tRow = new List<List<double>> { tCol };
                    var mmultCbvNegOneBCol = MatMultiply(tRow, matrixCbvNegOne);
                    double matNegValue = mmultCbvNegOneBCol[0][0];
                    changingZRow.Add(matNegValue - (-firstTab[0][j]));
                }

                // get the rhs optimal value
                var rhsCol = new List<double>();
                for (int i = 1; i < firstTab.Count; i++)
                {
                    rhsCol.Add(firstTab[i][firstTab[i].Count - 1]);
                }
                var rhsRow = new List<List<double>> { rhsCol };
                var rhsOptimal = MatMultiply(rhsRow, matrixCbvNegOne);
                double changingOptimal = rhsOptimal[0][0];

                // get the b values of the new changing table
                var changingBvRows = new List<List<double>>();
                for (int j = 0; j < firstTab[firstTab.Count - 1].Count; j++)       // all columns, including RHS
                {
                    var col = new List<double>();
                    for (int i = 1; i < firstTab.Count; i++)
                    {
                        col.Add(firstTab[i][j]);
                    }
                    var row = new List<List<double>> { col };                          // make 1×n
                    var product = MatMultiply(row, matrixBNegOne);  // (1×n) @ (n×n)
                    changingBvRows.Add(product[0]);    // flatten row
                }

                // transpose to match tableau format
                var transposeChangingB = MatTranspose(changingBvRows);

                // rebuild changing table from final tableau
                var revisedTab = firstTab.Select(row => row.Select(x => x).ToList()).ToList();

                changingZRow.Add(changingOptimal);
                revisedTab[0] = changingZRow;

                // fill in rows under Z row
                for (int i = 0; i < revisedTab.Count - 1; i++)
                {
                    for (int j = 0; j < revisedTab[i].Count; j++)
                    {
                        revisedTab[i + 1][j] = transposeChangingB[i][j];
                    }
                }

                this.revisedTab = revisedTab;

                Logger.WriteLine("Initial Table\n");
                for (int i = 0; i < headerStr.Count; i++)
                {
                    Logger.Write($"{headerStr[i],15} ");
                }
                Logger.WriteLine();
                for (int i = 0; i < firstTab.Count; i++)
                {
                    for (int j = 0; j < firstTab[i].Count; j++)
                    {
                        Logger.Write($"{firstTab[i][j],15:F4} ");
                    }
                    Logger.WriteLine();
                }

                Logger.WriteLine("\nOptimal Changing Table\n");
                for (int i = 0; i < headerStr.Count; i++)
                {
                    Logger.Write($"{headerStr[i],15} ");
                }
                Logger.WriteLine();
                for (int i = 0; i < revisedTab.Count; i++)
                {
                    for (int j = 0; j < revisedTab[i].Count; j++)
                    {
                        Logger.Write($"{revisedTab[i][j],15:F4} ");
                    }
                    Logger.WriteLine();
                }

                //return revisedTab;
            }

            public (double?, double?) GetNonBasicVariableRange(int varIndex)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return (null, null);
                }

                if (basicVarSpots.Contains(varIndex))
                {
                    Logger.WriteLine($"\nVariable {headerStr[varIndex]} is a basic variable!");
                    return (null, null);
                }

                // Get the column for this variable in the revised tableau
                var varColumn = new List<double>();
                for (int i = 1; i < revisedTab.Count; i++)
                {
                    varColumn.Add(revisedTab[i][varIndex]);
                }

                // Get current RHS values (basic variable values)
                var rhsValues = new List<double>();
                for (int i = 1; i < revisedTab.Count; i++)
                {
                    rhsValues.Add(revisedTab[i][revisedTab[i].Count - 1]);
                }

                // Calculate maximum increase: min{RHS[i] / -A[i,j]} for A[i,j] < 0
                double maxIncrease = double.PositiveInfinity;
                for (int i = 0; i < varColumn.Count; i++)
                {
                    if (varColumn[i] < -1e-10) // Negative coefficient
                    {
                        double ratio = rhsValues[i] / (-varColumn[i]);
                        if (ratio < maxIncrease)
                        {
                            maxIncrease = ratio;
                        }
                    }
                }

                Logger.WriteLine($"\nRange for Non-Basic Variable {headerStr[varIndex]}:");
                Logger.WriteLine($"Current value: 0");
                Logger.WriteLine($"Variable column: [{string.Join(", ", varColumn.Select(v => v.ToString("F4")))}]");
                Logger.WriteLine($"RHS values: [{string.Join(", ", rhsValues.Select(v => v.ToString("F4")))}]");

                if (maxIncrease == double.PositiveInfinity)
                {
                    Logger.WriteLine("Range: [0, +∞) - No upper bound (unbounded)");
                }
                else
                {
                    Logger.WriteLine($"Range: [0, {maxIncrease:F4}] - Limited by feasibility constraints");
                }

                return (0, maxIncrease);
            }

            public void ApplyNonBasicVariableChange(int varIndex, double newValue)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return;
                }

                if (basicVarSpots.Contains(varIndex))
                {
                    Logger.WriteLine($"Variable {headerStr[varIndex]} is a basic variable!");
                    return;
                }

                if (newValue < 0)
                {
                    Logger.WriteLine("Non-basic variable value cannot be negative!");
                    return;
                }

                Logger.WriteLine($"\nApplying change to Non-Basic Variable {headerStr[varIndex]}:");
                Logger.WriteLine($"Changing from 0 to {newValue}");

                // Get the column for this variable
                var varColumn = new List<double>();
                for (int i = 1; i < firstTab.Count; i++)
                {
                    varColumn.Add(firstTab[i][varIndex]);
                }

                // Calculate how basic variables change
                var bInvAj = MatMultiply(new List<List<double>> { varColumn }, matrixBNegOne)[0];

                Logger.WriteLine("\nImpact on basic variables:");
                var currentRhs = new List<double>();
                for (int i = 1; i < revisedTab.Count; i++)
                {
                    currentRhs.Add(revisedTab[i][revisedTab[i].Count - 1]);
                }

                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    double oldValue = currentRhs[i];
                    double newValueCalc = oldValue - bInvAj[i] * newValue;
                    Logger.WriteLine($"{headerStr[basicVarSpots[i]]}: {oldValue:F4} → {newValueCalc:F4}");
                }

                // Update objective function
                double reducedCost = revisedTab[0][varIndex];
                double objChange = reducedCost * newValue;
                double currentObj = revisedTab[0][revisedTab[0].Count - 1];
                double newObj = currentObj + objChange;
                Logger.WriteLine($"\nObjective function: {currentObj:F4} → {newObj:F4}");
            }

            public (double?, double?) GetBasicVariableRange(int varIndex)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return (null, null);
                }

                if (!basicVarSpots.Contains(varIndex))
                {
                    try
                    {
                        Logger.WriteLine($"Variable {headerStr[varIndex]} is not a basic variable!");
                    }
                    catch
                    {
                        return (null, null);
                    }
                    return (null, null);
                }

                // Find which basic variable this is
                int basicVarPosition = basicVarSpots.IndexOf(varIndex);
                double currentValue = revisedTab[basicVarPosition + 1][revisedTab[basicVarPosition + 1].Count - 1];

                // For basic variables, range is determined by when other basic variables become negative
                // We need to check the B^-1 matrix row for this basic variable
                var bInvRow = matrixBNegOne[basicVarPosition];
                var rhsValues = new List<double>();
                for (int i = 1; i < revisedTab.Count; i++)
                {
                    rhsValues.Add(revisedTab[i][revisedTab[i].Count - 1]);
                }

                // Calculate range based on dual feasibility
                double minDecrease = double.NegativeInfinity;
                double maxIncrease = double.PositiveInfinity;

                for (int i = 0; i < bInvRow.Count; i++)
                {
                    if (i != basicVarPosition && Math.Abs(bInvRow[i]) > 1e-10)
                    {
                        double ratio = rhsValues[i] / bInvRow[i];
                        if (bInvRow[i] > 0)
                        {
                            if (ratio < maxIncrease)
                                maxIncrease = ratio;
                        }
                        else
                        {
                            if (ratio > minDecrease)
                                minDecrease = ratio;
                        }
                    }
                }

                double lowerBound = Math.Max(0, currentValue + minDecrease);
                double upperBound = currentValue + maxIncrease;

                Logger.WriteLine($"\nRange for Basic Variable {headerStr[varIndex]}:");
                Logger.WriteLine($"Current value: {currentValue:F4}");
                Logger.WriteLine($"B^-1 row: [{string.Join(", ", bInvRow.Select(v => v.ToString("F4")))}]");
                Logger.WriteLine($"Range: [{lowerBound:F4}, {(upperBound == double.PositiveInfinity ? "∞" : upperBound.ToString("F4"))}]");

                return (lowerBound, upperBound);
            }

            public void ApplyBasicVariableChange(int varIndex, double newValue)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return;
                }

                if (!basicVarSpots.Contains(varIndex))
                {
                    Logger.WriteLine($"Variable {headerStr[varIndex]} is not a basic variable!");
                    return;
                }

                if (newValue < 0)
                {
                    Logger.WriteLine("Basic variable value cannot be negative in standard form!");
                    return;
                }

                int basicVarPosition = basicVarSpots.IndexOf(varIndex);
                double currentValue = revisedTab[basicVarPosition + 1][revisedTab[basicVarPosition + 1].Count - 1];

                Logger.WriteLine($"\nApplying change to Basic Variable {headerStr[varIndex]}:");
                Logger.WriteLine($"Changing from {currentValue:F4} to {newValue}");
                Logger.WriteLine("Note: Changing a basic variable directly may affect optimality.");
                Logger.WriteLine("This would require re-solving or using parametric programming techniques.");
            }

            public (double?, double?) GetRHSRange(int constraintIndex)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return (null, null);
                }

                if (constraintIndex >= revisedTab.Count - 1)
                {
                    Logger.WriteLine("Invalid constraint index!");
                    return (null, null);
                }

                double currentRhs = revisedTab[constraintIndex + 1][revisedTab[constraintIndex + 1].Count - 1];

                // Get the column from B^-1 corresponding to this constraint
                var bInvColumn = new List<double>();
                for (int i = 0; i < matrixBNegOne.Count; i++)
                {
                    if (constraintIndex < matrixBNegOne[i].Count)
                        bInvColumn.Add(matrixBNegOne[i][constraintIndex]);
                    else
                        bInvColumn.Add(0);
                }

                // Get current basic variable values
                var currentBasicValues = new List<double>();
                for (int i = 1; i < revisedTab.Count; i++)
                {
                    currentBasicValues.Add(revisedTab[i][revisedTab[i].Count - 1]);
                }

                // Calculate allowable change range
                double maxIncrease = double.PositiveInfinity;
                double maxDecrease = double.PositiveInfinity;

                for (int i = 0; i < bInvColumn.Count; i++)
                {
                    if (Math.Abs(bInvColumn[i]) > 1e-10)
                    {
                        double ratio = currentBasicValues[i] / bInvColumn[i];
                        if (bInvColumn[i] > 0) // Positive coefficient
                        {
                            if (ratio < maxDecrease)
                                maxDecrease = ratio;
                        }
                        else // Negative coefficient
                        {
                            if (-ratio < maxIncrease)
                                maxIncrease = -ratio;
                        }
                    }
                }

                double lowerBound = currentRhs - maxDecrease;
                double upperBound = currentRhs + maxIncrease;

                Logger.WriteLine($"\nRange for RHS of Constraint {constraintIndex + 1}:");
                Logger.WriteLine($"Current RHS value: {currentRhs:F4}");
                Logger.WriteLine($"B^-1 column {constraintIndex}: [{string.Join(", ", bInvColumn.Select(v => v.ToString("F4")))}]");
                Logger.WriteLine($"Current basic values: [{string.Join(", ", currentBasicValues.Select(v => v.ToString("F4")))}]");
                Logger.WriteLine($"Allowable decrease: {(maxDecrease == double.PositiveInfinity ? "∞" : maxDecrease.ToString("F4"))}");
                Logger.WriteLine($"Allowable increase: {(maxIncrease == double.PositiveInfinity ? "∞" : maxIncrease.ToString("F4"))}");
                Logger.WriteLine($"Range: [{(lowerBound == double.NegativeInfinity ? "-∞" : lowerBound.ToString("F4"))}, {(upperBound == double.PositiveInfinity ? "∞" : upperBound.ToString("F4"))}]");

                return (lowerBound, upperBound);
            }

            public void ApplyRHSChange(int constraintIndex, double newRHS)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return;
                }

                if (constraintIndex >= revisedTab.Count - 1)
                {
                    Logger.WriteLine("Invalid constraint index!");
                    return;
                }

                double currentRhs = revisedTab[constraintIndex + 1][revisedTab[constraintIndex + 1].Count - 1];
                double change = newRHS - currentRhs;

                Logger.WriteLine($"\nApplying RHS change to Constraint {constraintIndex + 1}:");
                Logger.WriteLine($"Changing RHS from {currentRhs:F4} to {newRHS}");
                Logger.WriteLine($"Change amount: {change:F4}");

                // Calculate impact on basic variables using B^-1
                if (constraintIndex < matrixBNegOne[0].Count)
                {
                    var bInvColumn = new List<double>();
                    for (int i = 0; i < matrixBNegOne.Count; i++)
                    {
                        bInvColumn.Add(matrixBNegOne[i][constraintIndex]);
                    }

                    Logger.WriteLine("\nImpact on basic variables:");
                    for (int i = 0; i < basicVarSpots.Count; i++)
                    {
                        double oldValue = revisedTab[i + 1][revisedTab[i + 1].Count - 1];
                        double newValue = oldValue + bInvColumn[i] * change;
                        Logger.WriteLine($"{headerStr[basicVarSpots[i]]}: {oldValue:F4} → {newValue:F4}");
                    }

                    // Calculate shadow price (dual variable) for this constraint
                    // Shadow price is the coefficient in the z-row for the slack variable of this constraint
                    double shadowPrice = 0;
                    int slackVarIndex = objFunc.Count + constraintIndex; // Assuming slack variables start after decision variables
                    if (slackVarIndex < revisedTab[0].Count - 1)
                    {
                        shadowPrice = revisedTab[0][slackVarIndex];
                    }

                    double objChange = shadowPrice * change;
                    double currentObj = revisedTab[0][revisedTab[0].Count - 1];
                    double newObj = currentObj + objChange;

                    Logger.WriteLine($"\nShadow price for constraint {constraintIndex + 1}: {shadowPrice:F4}");
                    Logger.WriteLine($"Objective function: {currentObj:F4} → {newObj:F4} (change: {objChange:F4})");
                }
            }

            public (double?, double?) GetNonBasicColumnRange(int varIndex, int rowIndex)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return (null, null);
                }

                if (basicVarSpots.Contains(varIndex))
                {
                    Logger.WriteLine($"Variable {headerStr[varIndex]} is a basic variable!");
                    return (null, null);
                }

                if (rowIndex >= revisedTab.Count - 1)
                {
                    Logger.WriteLine("Invalid row index!");
                    return (null, null);
                }

                double currentValue = revisedTab[rowIndex + 1][varIndex];

                // For coefficient changes, we need to maintain optimality
                // Current reduced cost
                double currentReducedCost = revisedTab[0][varIndex];

                // Get the corresponding row from B^-1
                var bInvRow = new List<double>();
                if (rowIndex < matrixBNegOne.Count)
                {
                    for (int j = 0; j < matrixBNegOne[rowIndex].Count; j++)
                    {
                        bInvRow.Add(matrixBNegOne[rowIndex][j]);
                    }
                }

                // Get the corresponding coefficient from cbv
                double cbvCoeff = 0;
                if (rowIndex < matrixCbv.Count)
                {
                    cbvCoeff = matrixCbv[rowIndex][0];
                }

                // Calculate range where reduced cost remains non-positive (for maximization)
                // New reduced cost = current reduced cost - cbv[i] * change
                // For optimality: new reduced cost <= 0
                // So: current reduced cost - cbv[i] * change <= 0
                // change >= current reduced cost / cbv[i]

                double maxChange = double.PositiveInfinity;
                double minChange = double.NegativeInfinity;

                if (Math.Abs(cbvCoeff) > 1e-10)
                {
                    double criticalChange = currentReducedCost / cbvCoeff;
                    if (cbvCoeff > 0)
                    {
                        maxChange = criticalChange;
                    }
                    else
                    {
                        minChange = criticalChange;
                    }
                }

                double lowerBound = currentValue + minChange;
                double upperBound = currentValue + maxChange;

                Logger.WriteLine($"\nRange for coefficient [Row {rowIndex + 1}, {headerStr[varIndex]}]:");
                Logger.WriteLine($"Current value: {currentValue:F4}");
                Logger.WriteLine($"Current reduced cost: {currentReducedCost:F4}");
                Logger.WriteLine($"Corresponding cbv coefficient: {cbvCoeff:F4}");
                Logger.WriteLine($"B^-1 row {rowIndex}: [{string.Join(", ", bInvRow.Select(v => v.ToString("F4")))}]");
                Logger.WriteLine($"Range: [{(lowerBound == double.NegativeInfinity ? "-∞" : lowerBound.ToString("F4"))}, {(upperBound == double.PositiveInfinity ? "∞" : upperBound.ToString("F4"))}]");

                return (lowerBound, upperBound);
            }

            public void ApplyNonBasicColumnChange(int varIndex, int rowIndex, double newValue)
            {
                if (revisedTab == null)
                {
                    Logger.WriteLine("Please run the optimization first!");
                    return;
                }

                if (basicVarSpots.Contains(varIndex))
                {
                    Logger.WriteLine($"Variable {headerStr[varIndex]} is a basic variable!");
                    return;
                }

                if (rowIndex >= revisedTab.Count - 1)
                {
                    Logger.WriteLine("Invalid row index!");
                    return;
                }

                double currentValue = revisedTab[rowIndex + 1][varIndex];
                double change = newValue - currentValue;

                Logger.WriteLine($"\nApplying change to coefficient [Row {rowIndex + 1}, {headerStr[varIndex]}]:");
                Logger.WriteLine($"Changing from {currentValue:F4} to {newValue}");
                Logger.WriteLine($"Change amount: {change:F4}");
                Logger.WriteLine("This requires updating the tableau and checking optimality.");
            }

            public void DisplayCurrentTableau()
            {
                Logger.WriteLine("\nCurrent Optimal Tableau:");
                Logger.WriteLine(new string('-', 50));
                for (int i = 0; i < headerStr.Count; i++)
                {
                    Logger.Write($"{headerStr[i],15} ");
                }
                Logger.WriteLine();
                for (int i = 0; i < revisedTab.Count; i++)
                {
                    for (int j = 0; j < revisedTab[i].Count; j++)
                    {
                        Logger.Write($"{revisedTab[i][j],15:F4} ");
                    }
                    Logger.WriteLine();
                }
            }

            public void HandleNonBasicVariableRange(int varIdx = -1)
            {
                Logger.WriteLine("\nAvailable variables:");
                for (int i = 0; i < headerStr.Count - 1; i++)  // Exclude RHS
                {
                    string status = basicVarSpots.Contains(i) ? "Basic" : "Non-Basic";
                    Logger.WriteLine($"{i}: {headerStr[i]} ({status})");
                }

                // if (int.TryParse(Logger.ReadLine(), out int varIdx))
                if (varIdx != -1)
                {
                    if (varIdx >= 0 && varIdx < headerStr.Count - 1)
                    {
                        GetNonBasicVariableRange(varIdx);
                    }
                    else
                    {
                        Logger.WriteLine("\nInvalid variable index!");
                    }
                }
                else
                {
                    Logger.Write("\nEnter variable index: \n");
                    Logger.WriteLine("\nPlease enter a valid number!");
                }
            }

            public void HandleNonBasicVariableChange(float val = -1, int varIdx = -1)
            {
                Logger.WriteLine("\nAvailable non-basic variables:");
                for (int i = 0; i < headerStr.Count - 1; i++)
                {
                    if (!basicVarSpots.Contains(i))
                    {
                        Logger.WriteLine($"{i}: {headerStr[i]}");
                    }
                }

                Logger.WriteLine("\n");
                if (varIdx != -1 && val != -1)
                {
                    Logger.WriteLine("Enter new value: ");

                    if (varIdx >= 0 && varIdx < headerStr.Count - 1)
                    {
                        ApplyNonBasicVariableChange(varIdx, val);
                    }
                    else
                    {
                        Logger.WriteLine("Invalid variable index!");
                    }
                }
                else
                {
                    Logger.WriteLine("Enter variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public void HandleBasicVariableRange(int varIdx = -1)
            {
                Logger.WriteLine("\nAvailable basic variables:");
                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    int varIdxL = basicVarSpots[i];
                    Logger.WriteLine($"{varIdxL}: {headerStr[varIdxL]}");
                }

                Logger.WriteLine("\n");
                if (varIdx != -1)
                {
                    GetBasicVariableRange(varIdx);
                }
                else
                {
                    Logger.WriteLine("Enter variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public void HandleBasicVariableChange(float val = -1, int varIdx = -1)
            {
                Logger.WriteLine("\nAvailable basic variables:");
                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    int varIdxL = basicVarSpots[i];
                    double currentVal = revisedTab[i + 1][revisedTab[i + 1].Count - 1];
                    Logger.WriteLine($"{varIdxL}: {headerStr[varIdxL]} (current: {currentVal:F4})");
                }

                Logger.WriteLine("\n");
                if (varIdx != -1 && val != -1)
                {
                    Logger.WriteLine("Enter new value: ");

                    if (varIdx >= 0 && varIdx < headerStr.Count - 1)
                    {
                        ApplyBasicVariableChange(varIdx, val);
                    }
                    else
                    {
                        Logger.WriteLine("Invalid variable index!");
                    }
                }
                else
                {
                    Logger.WriteLine("Enter variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public void HandleRHSRange(int constraintIdx = -1)
            {
                Logger.WriteLine($"\nAvailable constraints (0 to {revisedTab.Count - 2}):");
                for (int i = 0; i < revisedTab.Count - 1; i++)
                {
                    double currentRhs = revisedTab[i + 1][revisedTab[i + 1].Count - 1];
                    Logger.WriteLine($"Constraint {i}: Current RHS = {currentRhs:F4}");
                }

                Logger.WriteLine("\n");
                if (constraintIdx != -1)
                {
                    GetRHSRange(constraintIdx);
                }
                else
                {
                    Logger.WriteLine("Enter constraint index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }

                //Logger.Write("Enter constraint index: ");
                //if (int.TryParse(Logger.ReadLine(), out int constraintIdx))
                //{
                //    GetRHSRange(constraintIdx);
                //}
                //else
                //{
                //    Logger.WriteLine("Please enter a valid number!");
                //}
            }

            public void HandleRHSChange(float val = -1, int varIdx = -1)
            {
                Logger.WriteLine($"\nAvailable constraints (0 to {revisedTab.Count - 2}):");
                for (int i = 0; i < revisedTab.Count - 1; i++)
                {
                    double currentRhs = revisedTab[i + 1][revisedTab[i + 1].Count - 1];
                    Logger.WriteLine($"Constraint {i}: Current RHS = {currentRhs:F4}");
                }

                Logger.WriteLine("\n");
                if (varIdx != -1 && val != -1)
                {

                    Logger.Write("Enter new RHS value: ");
                    if (varIdx >= 0 && varIdx < headerStr.Count - 1)
                    {
                        ApplyBasicVariableChange(varIdx, val);
                    }
                    else
                    {
                        Logger.WriteLine("Invalid variable index!");
                    }
                }
                else
                {
                    Logger.WriteLine("Enter variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public void HandleNonBasicColumnRange(float rowIdx = -1, int varIdx = -1)
            {
                int rowIdxIn = (int)rowIdx;

                Logger.WriteLine("\nAvailable non-basic variables:");
                for (int i = 0; i < headerStr.Count - 1; i++)
                {
                    if (!basicVarSpots.Contains(i))
                    {
                        Logger.WriteLine($"{i}: {headerStr[i]}");
                    }
                }


                if (varIdx != -1 && rowIdxIn != -1)
                {
                    if (!basicVarSpots.Contains(varIdx))
                    {
                        Logger.WriteLine($"\nRows for variable {headerStr[varIdx]}:");
                        for (int i = 0; i < revisedTab.Count - 1; i++)
                        {
                            double currentVal = revisedTab[i + 1][varIdx];
                            Logger.WriteLine($"Row {i}: {currentVal:F4}");
                        }

                        Logger.Write("Enter row index: ");
                        if (rowIdxIn != -1)
                        {
                            GetNonBasicColumnRange(varIdx, rowIdxIn);
                        }
                        else
                        {
                            Logger.WriteLine("Please enter a valid number!");
                        }
                    }
                    else
                    {
                        Logger.WriteLine("Selected variable is basic!");
                    }
                }
                else
                {
                    Logger.Write("Enter non-basic variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public void HandleNonBasicColumnChange(int rowIdx = -1, float val = -1, int varIdx = -1)
            {
                Logger.WriteLine("\nAvailable non-basic variables:");
                for (int i = 0; i < headerStr.Count - 1; i++)
                {
                    if (!basicVarSpots.Contains(i))
                    {
                        Logger.WriteLine($"{i}: {headerStr[i]}");
                    }
                }

                Logger.WriteLine("\n");

                if (varIdx != -1 && rowIdx != -1 && val != -1)
                {
                    if (!basicVarSpots.Contains(varIdx))
                    {
                        try
                        {
                            Logger.WriteLine($"\nRows for variable {headerStr[varIdx]}:");
                            for (int i = 0; i < revisedTab.Count - 1; i++)
                            {
                                double currentVal = revisedTab[i + 1][varIdx];
                                Logger.WriteLine($"Row {i}: {currentVal:F4}");
                            }
                        }
                        catch
                        {
                            Logger.WriteLine("\nInvalid index!");
                        }

                        Logger.Write("Enter row index: ");
                        if (rowIdx != -1)
                        {
                            Logger.Write("Enter new coefficient value: ");
                            if (val != -1)
                            {
                                ApplyNonBasicColumnChange(varIdx, rowIdx, val);
                            }
                            else
                            {
                                Logger.WriteLine("Please enter a valid number!");
                            }
                        }
                        else
                        {
                            Logger.WriteLine("Please enter a valid number!");
                        }
                    }
                    else
                    {
                        Logger.WriteLine("Selected variable is basic!");
                    }
                }
                else
                {
                    Logger.WriteLine("Enter non-basic variable index: ");
                    Logger.WriteLine("Please enter a valid number!");
                }
            }

            public List<double> DoAddActivity(
            List<double> activity)
            {
                // Convert activity (skip first element) into a column matrix
                var matrixAct = MatFromColVector(activity.Skip(1).ToList());

                // c = matrixAct^T * matrixCbvNegOne
                var c = MatMultiply(MatTranspose(matrixAct), matrixCbvNegOne);

                // Top value of the objective row
                double cTop = c[0][0] - activity[0];

                // b = matrixAct^T * matrixBNegOne
                var b = MatMultiply(MatTranspose(matrixAct), matrixBNegOne);

                // Flatten the last row of b for display
                var displayCol = new List<double> { cTop };
                displayCol.AddRange(b.Last());


                Logger.WriteLine("Display Column: " + string.Join(", ", displayCol));

                return displayCol;
            }

            public double RoundValue(object value)
            {
                try
                {
                    return Math.Round(Convert.ToDouble(value), 4);
                }
                catch
                {
                    return Convert.ToDouble(value);
                }
            }

            public List<List<double>> RoundMatrix(List<List<double>> matrix)
            {
                if (matrix != null && matrix.Count > 0)
                {
                    if (matrix[0] is List<double>)
                    {
                        // 2D matrix
                        return matrix.Select(row => row.Select(val => RoundValue(val)).ToList()).ToList();
                    }
                    else
                    {
                        // 1D array
                        return new List<List<double>> { matrix[0].Select(val => RoundValue(val)).ToList() };
                    }
                }
                else
                {
                    return new List<List<double>> { new List<double> { RoundValue(matrix) } };
                }
            }

            public void PrintTableau(List<List<double>> tableau, string title = "Tableau")
            {
                var tempHeaderStr = new List<string>();
                for (int i = 0; i < objFunc.Count; i++)
                {
                    tempHeaderStr.Add($"x{i + 1}");
                }
                for (int i = 0; i < (tableau.Last().Count - objFunc.Count - 1); i++)
                {
                    tempHeaderStr.Add($"s{i + 1}");
                }
                tempHeaderStr.Add("rhs");

                if (isConsoleOutput)
                {
                    Logger.WriteLine($"\n{title}");
                    foreach (var header in tempHeaderStr)
                    {
                        Logger.Write($"{header,8}  ");
                    }
                    Logger.WriteLine();

                    for (int i = 0; i < tableau.Count; i++)
                    {
                        for (int j = 0; j < tableau[i].Count; j++)
                        {
                            Logger.Write($"{RoundValue(tableau[i][j]),8:0.0000}  ");
                        }
                        Logger.WriteLine();
                    }
                    Logger.WriteLine();
                }
            }

            public (List<List<double>>, List<List<double>>) DoAddConstraint(List<List<double>> addedConstraints, List<List<double>> overRideTab = null)
            {
                if (overRideTab != null)
                {
                    var changingTable = overRideTab.Select(row => row.ToList()).ToList();
                    changingTable = RoundMatrix(changingTable);
                    var tempTabs = new List<List<List<double>>> { changingTable };
                    var basicVarSpots = this.basicVarSpots;
                }
                else
                {
                    Logger.WriteLine("needs an input table");
                    return (null, null);
                }

                var newTab = overRideTab.Select(row => row.ToList()).ToList();

                // Add new constraint rows to the tableau
                for (int k = 0; k < addedConstraints.Count; k++)
                {
                    for (int i = 0; i < overRideTab.Count; i++)
                    {
                        newTab[i].Insert(newTab[i].Count - 1, 0.0);
                    }

                    var newCon = new List<double>();
                    for (int i = 0; i < overRideTab[0].Count + addedConstraints.Count; i++)
                    {
                        newCon.Add(0.0);
                    }

                    for (int i = 0; i < addedConstraints[k].Count - 2; i++)
                    {
                        newCon[i] = RoundValue(addedConstraints[k][i]);
                    }

                    newCon[newCon.Count - 1] = RoundValue(addedConstraints[k][addedConstraints[k].Count - 2]);

                    int slackSpot = ((newCon.Count - addedConstraints.Count) - 1) + k;
                    if (addedConstraints[k].Last() == 1)  // >= constraint
                    {
                        newCon[slackSpot] = -1.0;
                    }
                    else
                    {
                        newCon[slackSpot] = 1.0;
                    }

                    newTab.Add(newCon);
                }

                newTab = RoundMatrix(newTab);
                PrintTableau(newTab, "unfixed tab");

                var displayTab = newTab.Select(row => row.ToList()).ToList();

                for (int k = 0; k < addedConstraints.Count; k++)
                {
                    int constraintRowIndex = newTab.Count - addedConstraints.Count + k;

                    foreach (var colIndex in basicVarSpots)
                    {
                        double coefficientInNewRow = RoundValue(displayTab[constraintRowIndex][colIndex]);

                        if (Math.Abs(coefficientInNewRow) > 1e-6)
                        {
                            int? pivotRow = null;
                            for (int rowIndex = 0; rowIndex < displayTab.Count - addedConstraints.Count; rowIndex++)
                            {
                                if (Math.Abs(RoundValue(displayTab[rowIndex][colIndex]) - 1.0) <= 1e-6)
                                {
                                    pivotRow = rowIndex;
                                    break;
                                }
                            }

                            if (pivotRow.HasValue)
                            {
                                int constraintType = (int)addedConstraints[k].Last();
                                bool autoReverse = (constraintType == 1);

                                for (int col = 0; col < displayTab[0].Count; col++)
                                {
                                    double pivotVal = RoundValue(displayTab[pivotRow.Value][col]);
                                    double constraintVal = RoundValue(displayTab[constraintRowIndex][col]);

                                    double newVal;
                                    if (autoReverse)
                                        newVal = pivotVal - coefficientInNewRow * constraintVal;
                                    else
                                        newVal = constraintVal - coefficientInNewRow * pivotVal;

                                    displayTab[constraintRowIndex][col] = RoundValue(newVal);
                                }
                            }
                        }
                    }
                }

                displayTab = RoundMatrix(displayTab);
                PrintTableau(displayTab, "fixed tab");

                return (displayTab, newTab);
            }

            private List<double> _objFunc;
            public List<double> GetObjFunc()
            {
                return _objFunc;
            }


            private List<List<double>> _constraints;
            public List<List<double>> GetConstraints()
            {
                return _constraints;
            }

            private bool _isMin;
            public bool GetIsMin()
            {
                return _isMin;
            }


            public int GetColCount()
            {
                // int numCols = revisedTab[0].Count;

                int numCols = _constraints[0].Count;
                return numCols;
            }

            public List<List<double>> GetRevisedTab()
            {
                return revisedTab;
            }

            public (List<double> objFunc, List<List<double>> constraints) SetUpProblem(
            List<double> objFunc,
            List<List<double>> constraints,
            List<VariableSignType> varSigns = null
    )
            {
                int numConstraints = objFunc.Count;      // e.g. 6
                int vectorLength = objFunc.Count + 3;    // columns including slack + RHS

                for (int i = 0; i < numConstraints; i++)
                {
                    if (varSigns != null && varSigns[i] == VariableSignType.Binary)
                    {
                        var row = new List<double>(new double[vectorLength]);

                        // Put a "1" at column i (the decision variable column)
                        row[i] = 1;

                        // Put a "1" in the slack column (vectorLength - 2)
                        row[vectorLength - 2] = 1;

                        // RHS (last element) stays 0
                        constraints.Add(row);
                    }
                }

                return (objFunc, constraints);
            }

            public void RunSensitivityAnalysis(List<double> objFuncPassed, List<List<double>> constraintsPassed, bool isMinPassed, List<VariableSignType> varSigns = null)
            {

                objFunc = objFuncPassed.ToList();
                constraints = constraintsPassed.Select(x => x.ToList()).ToList();
                bool isMin = isMinPassed;

                (objFunc, constraints) = SetUpProblem(objFunc, constraints, varSigns);

                DoPreliminaries(objFunc, constraints, isMin);

                _objFunc = objFunc;
                _constraints = constraints;
                _isMin = isMin;
            }
        
    }
}


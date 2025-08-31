using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{

    public enum RevisedVariableSignType
    {
        Positive,      // '+' - x >= 0
        Negative,      // '-' - x <= 0
        Unrestricted,  // 'urs' - x unrestricted
        Integer,       // 'int' - x integer
        Binary         // 'bin' - x binary
    }

    public class RevisedVariableTransformation
    {
        public string Type { get; set; }
        public int OriginalIndex { get; set; }
        public int TransformedIndex { get; set; }
        public int? TransformedIndexNeg { get; set; } // For unrestricted variables

        public RevisedVariableTransformation(string type, int originalIndex, int transformedIndex, int? transformedIndexNeg = null)
        {
            Type = type;
            OriginalIndex = originalIndex;
            TransformedIndex = transformedIndex;
            TransformedIndexNeg = transformedIndexNeg;
        }

        public class RevisedPrimalSimplex
        {
            private bool isConsoleOutput;

            private DualSimplex dual;
            private List<double> objFunc;
            private List<List<double>> constraints;
            private List<RevisedVariableTransformation> variableTransformations;

            public RevisedPrimalSimplex(bool isConsoleOutput = false)
            {
                this.isConsoleOutput = isConsoleOutput;
                dual = new DualSimplex();
                objFunc = new List<double> { 0.0, 0.0 };
                constraints = new List<List<double>>
            {
                new List<double> { 0.0, 0.0, 0.0, 0.0 }
            };
                variableTransformations = new List<RevisedVariableTransformation>();
            }

            public (List<double> transformedObjFunc, List<List<double>> transformedConstraints)
            TransformVariableSignRestrictions(List<double> objFunc, List<List<double>> constraints, List<VariableSignType> varSigns)
            {
                var transformedObjFunc = new List<double>();
                var transformedConstraints = constraints.Select(row => new List<double>()).ToList();
                variableTransformations.Clear();

                int newVarIndex = 0;
                var binaryVariableIndices = new List<int>(); // Track which variables are binary

                for (int i = 0; i < objFunc.Count; i++)
                {
                    switch (varSigns[i])
                    {
                        case VariableSignType.Positive:
                            // x >= 0 - no transformation needed
                            transformedObjFunc.Add(objFunc[i]);
                            variableTransformations.Add(new RevisedVariableTransformation("positive", i, newVarIndex));

                            // Copy coefficients to transformed constraints
                            for (int j = 0; j < constraints.Count; j++)
                            {
                                if (transformedConstraints[j].Count <= newVarIndex)
                                {
                                    while (transformedConstraints[j].Count <= newVarIndex)
                                        transformedConstraints[j].Add(0);
                                }
                                transformedConstraints[j][newVarIndex] = constraints[j][i];
                            }
                            newVarIndex++;
                            break;

                        case VariableSignType.Negative:
                            // x <= 0 -> substitute x = -x', where x' >= 0
                            transformedObjFunc.Add(-objFunc[i]);
                            variableTransformations.Add(new RevisedVariableTransformation("negative", i, newVarIndex));

                            // Copy negative coefficients to transformed constraints
                            for (int j = 0; j < constraints.Count; j++)
                            {
                                if (transformedConstraints[j].Count <= newVarIndex)
                                {
                                    while (transformedConstraints[j].Count <= newVarIndex)
                                        transformedConstraints[j].Add(0);
                                }
                                transformedConstraints[j][newVarIndex] = -constraints[j][i];
                            }
                            newVarIndex++;
                            break;

                        case VariableSignType.Unrestricted:
                            // x unrestricted -> x = x1 - x2, where x1, x2 >= 0
                            transformedObjFunc.Add(objFunc[i]);   // for x1
                            transformedObjFunc.Add(-objFunc[i]);  // for x2
                            variableTransformations.Add(new RevisedVariableTransformation("unrestricted", i, newVarIndex, newVarIndex + 1));

                            // Add coefficients for both x1 and x2
                            for (int j = 0; j < constraints.Count; j++)
                            {
                                // Ensure we have enough space
                                while (transformedConstraints[j].Count <= newVarIndex + 1)
                                    transformedConstraints[j].Add(0);

                                transformedConstraints[j][newVarIndex] = constraints[j][i];      // x1 coefficient
                                transformedConstraints[j][newVarIndex + 1] = -constraints[j][i]; // x2 coefficient
                            }
                            newVarIndex += 2;
                            break;

                        case VariableSignType.Integer:
                            // For integer variables, treat as positive for now (integer constraints handled separately)
                            transformedObjFunc.Add(objFunc[i]);
                            variableTransformations.Add(new RevisedVariableTransformation("integer", i, newVarIndex));

                            for (int j = 0; j < constraints.Count; j++)
                            {
                                if (transformedConstraints[j].Count <= newVarIndex)
                                {
                                    while (transformedConstraints[j].Count <= newVarIndex)
                                        transformedConstraints[j].Add(0);
                                }
                                transformedConstraints[j][newVarIndex] = constraints[j][i];
                            }
                            newVarIndex++;
                            break;

                        case VariableSignType.Binary:
                            // For binary variables, treat as positive with additional constraints (0 <= x <= 1)
                            transformedObjFunc.Add(objFunc[i]);
                            variableTransformations.Add(new RevisedVariableTransformation("binary", i, newVarIndex));
                            binaryVariableIndices.Add(newVarIndex); // Track this binary variable

                            for (int j = 0; j < constraints.Count; j++)
                            {
                                if (transformedConstraints[j].Count <= newVarIndex)
                                {
                                    while (transformedConstraints[j].Count <= newVarIndex)
                                        transformedConstraints[j].Add(0);
                                }
                                transformedConstraints[j][newVarIndex] = constraints[j][i];
                            }
                            newVarIndex++;
                            break;
                    }
                }

                // Copy RHS and constraint type indicators
                for (int i = 0; i < transformedConstraints.Count; i++)
                {
                    // Ensure all constraint rows have the same length
                    while (transformedConstraints[i].Count < newVarIndex)
                        transformedConstraints[i].Add(0);

                    // Add RHS
                    transformedConstraints[i].Add(constraints[i][constraints[i].Count - 2]); // RHS
                    transformedConstraints[i].Add(constraints[i][constraints[i].Count - 1]); // Constraint type
                }

                // Add binary constraints (x <= 1) for each binary variable
                // This creates the diagonal pattern you requested
                foreach (int binaryVarIndex in binaryVariableIndices)
                {
                    var binaryConstraint = new List<double>();

                    // Initialize constraint with zeros for all variables
                    for (int k = 0; k < newVarIndex; k++)
                    {
                        binaryConstraint.Add(0);
                    }

                    // Set coefficient to 1 for the binary variable (diagonal pattern)
                    binaryConstraint[binaryVarIndex] = 1;

                    // Add RHS = 1 and constraint type (assuming <= is represented by some value, e.g., -1 or 2)
                    binaryConstraint.Add(1);  // RHS = 1 for x <= 1 constraint
                    binaryConstraint.Add(-1); // Constraint type for <= (adjust this value based on your system)

                    transformedConstraints.Add(binaryConstraint);
                }

                return (transformedObjFunc, transformedConstraints);
            }

            public List<double> TransformSolutionBack(List<double> transformedSolution)
            {
                var originalSolution = new List<double>();

                // Initialize with zeros for all original variables
                for (int i = 0; i < variableTransformations.Where(t => t.Type != "slack" && t.Type != "excess").Max(t => t.OriginalIndex) + 1; i++)
                {
                    originalSolution.Add(0);
                }

                foreach (var transformation in variableTransformations)
                {
                    switch (transformation.Type)
                    {
                        case "positive":
                        case "integer":
                        case "binary":
                            if (transformation.TransformedIndex < transformedSolution.Count)
                            {
                                originalSolution[transformation.OriginalIndex] = transformedSolution[transformation.TransformedIndex];
                            }
                            break;

                        case "negative":
                            if (transformation.TransformedIndex < transformedSolution.Count)
                            {
                                originalSolution[transformation.OriginalIndex] = -transformedSolution[transformation.TransformedIndex];
                            }
                            break;

                        case "unrestricted":
                            if (transformation.TransformedIndex < transformedSolution.Count &&
                                transformation.TransformedIndexNeg.HasValue &&
                                transformation.TransformedIndexNeg.Value < transformedSolution.Count)
                            {
                                originalSolution[transformation.OriginalIndex] =
                                    transformedSolution[transformation.TransformedIndex] -
                                    transformedSolution[transformation.TransformedIndexNeg.Value];
                            }
                            break;
                    }
                }

                return originalSolution;
            }

            public void PrintTransformations()
            {
                if (isConsoleOutput)
                {
                    OutputWriter.WriteLine("\nVariable Transformations:");
                    OutputWriter.WriteLine("========================");
                    OutputWriter.WriteLine("\nVariable Transformations:");
                    OutputWriter.WriteLine("========================");
                    foreach (var transformation in variableTransformations)
                    {
                        switch (transformation.Type)
                        {
                            case "positive":
                            case "integer":
                            case "binary":
                                OutputWriter.WriteLine($"x{transformation.OriginalIndex + 1} ({transformation.Type}) -> x'{transformation.TransformedIndex + 1}");
                                OutputWriter.WriteLine($"x{transformation.OriginalIndex + 1} ({transformation.Type}) -> x'{transformation.TransformedIndex + 1}");
                                break;
                            case "negative":
                                OutputWriter.WriteLine($"x{transformation.OriginalIndex + 1} (negative) -> -x'{transformation.TransformedIndex + 1}");
                                break;
                            case "unrestricted":
                                OutputWriter.WriteLine($"x{transformation.OriginalIndex + 1} (unrestricted) -> x'{transformation.TransformedIndex + 1} - x'{transformation.TransformedIndexNeg + 1}");
                                break;
                        }
                    }
                    OutputWriter.WriteLine();
                }
            }

            public List<List<double>> DoFormulationOperation(List<double> objFunc, List<List<double>> constraints)
            {
                int excessCount = 0;
                int slackCount = 0;

                // Count excess and slack variables
                for (int i = 0; i < constraints.Count; i++)
                {
                    if (constraints[i][constraints[i].Count - 1] == 1)
                        excessCount++;
                    else
                        slackCount++;
                }

                // Multiply by -1 for excess constraints
                for (int i = 0; i < constraints.Count; i++)
                {
                    if (constraints[i][constraints[i].Count - 1] == 1)
                    {
                        for (int j = 0; j < constraints[i].Count; j++)
                        {
                            constraints[i][j] = -1 * constraints[i][j];
                        }
                    }
                }

                // Remove last column (constraint type indicator)
                for (int i = 0; i < constraints.Count; i++)
                {
                    constraints[i].RemoveAt(constraints[i].Count - 1);
                }

                int tableSizeH = constraints.Count + 1;
                int tableSizeW = excessCount + slackCount + 1 + objFunc.Count;

                // Initialize operation table
                var opTable = new List<List<double>>();
                for (int i = 0; i < tableSizeH; i++)
                {
                    opTable.Add(new List<double>(new double[tableSizeW]));
                }

                // Set objective function row
                for (int i = 0; i < objFunc.Count; i++)
                {
                    opTable[0][i] = -objFunc[i];
                }

                // Set constraint rows
                for (int i = 0; i < constraints.Count; i++)
                {
                    for (int j = 0; j < constraints[i].Count - 1; j++)
                    {
                        opTable[i + 1][j] = constraints[i][j];
                    }
                    opTable[i + 1][opTable[i + 1].Count - 1] = constraints[i][constraints[i].Count - 1];
                }

                // Add slack and excess variables (identity matrix)
                for (int i = 1; i < opTable.Count; i++)
                {
                    for (int j = objFunc.Count; j < opTable[i].Count - 1; j++)
                    {
                        if (j == i + objFunc.Count - 1)
                            opTable[i][j] = 1;
                    }
                }

                return opTable;
            }

            public List<List<double>> MatTranspose(List<List<double>> matrix)
            {
                int rows = matrix.Count;
                int cols = matrix[0].Count;
                var result = new List<List<double>>();

                for (int j = 0; j < cols; j++)
                {
                    var row = new List<double>();
                    for (int i = 0; i < rows; i++)
                    {
                        row.Add(matrix[i][j]);
                    }
                    result.Add(row);
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
                        row.Add(i == j ? 1 : 0);
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

                // Create augmented matrix [A|I]
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

                    // Handle zero pivot by swapping rows
                    if (Math.Abs(pivot) < 1e-10)
                    {
                        bool swapped = false;
                        for (int r = i + 1; r < n; r++)
                        {
                            if (Math.Abs(aug[r][i]) > 1e-10)
                            {
                                var temp = aug[i];
                                aug[i] = aug[r];
                                aug[r] = temp;
                                pivot = aug[i][i];
                                swapped = true;
                                break;
                            }
                        }
                        if (!swapped)
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

                // Extract inverse from augmented matrix
                var result = new List<List<double>>();
                for (int i = 0; i < n; i++)
                {
                    result.Add(aug[i].GetRange(n, n));
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
                OutputWriter.WriteLine(name);
                var transposed = MatTranspose(M);
                foreach (var row in transposed)
                {
                    foreach (var val in row)
                    {
                        OutputWriter.Write($"{val,6:F2} ");
                    }
                    OutputWriter.WriteLine();
                }
                OutputWriter.WriteLine();
            }

            public void DoPreliminaries(List<double> objFunc, List<List<double>> constraints, bool isMin, List<VariableSignType> varSigns)
            {
                // Transform variables according to their sign restrictions
                var (transformedObjFunc, transformedConstraints) = TransformVariableSignRestrictions(objFunc, constraints, varSigns);

                if (isConsoleOutput)
                {
                    PrintTransformations();

                    OutputWriter.WriteLine("Transformed Objective Function:");
                    OutputWriter.WriteLine($"Maximize {string.Join(" + ", transformedObjFunc.Select((c, i) => $"{c}x'{i + 1}"))}");
                    OutputWriter.WriteLine();

                    OutputWriter.WriteLine("Transformed Constraints:");
                    for (int i = 0; i < transformedConstraints.Count; i++)
                    {
                        var constraintTerms = new List<string>();
                        for (int j = 0; j < transformedConstraints[i].Count - 2; j++)
                        {
                            if (transformedConstraints[i][j] != 0)
                            {
                                constraintTerms.Add($"{transformedConstraints[i][j]}x'{j + 1}");
                            }
                        }
                        string constraintStr = string.Join(" + ", constraintTerms);
                        string operator_str = transformedConstraints[i][transformedConstraints[i].Count - 1] == 0 ? "<=" : ">=";
                        OutputWriter.WriteLine($"  {constraintStr} {operator_str} {transformedConstraints[i][transformedConstraints[i].Count - 2]}");
                    }
                    OutputWriter.WriteLine();
                }

                // Create deep copies for dual simplex
                var tObjFunc = new List<double>(transformedObjFunc);
                var tConstraints = transformedConstraints.Select(row => new List<double>(row)).ToList();

                // Call dual simplex with transformed problem
                var (tableaus, changingVars, optimalSolution, param4, param5, headerStr) =
                    dual.DoDualSimplex(tObjFunc, tConstraints, isMin);

                // Get the spots of the basic variables
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

                // Get the columns of the basic variables
                var basicVarCols = new List<List<double>>();
                for (int i = 0; i < tableaus[tableaus.Count - 1][tableaus[tableaus.Count - 1].Count - 1].Count; i++)
                {
                    if (basicVarSpots.Contains(i))
                    {
                        var tLst = new List<double>();
                        for (int j = 0; j < tableaus[tableaus.Count - 1].Count; j++)
                        {
                            tLst.Add(tableaus[tableaus.Count - 1][j][i]);
                        }
                        basicVarCols.Add(tLst);
                    }
                }

                // Sort the cbv according to the basic var positions
                var zippedCbv = basicVarCols.Zip(basicVarSpots, (col, spot) => new { col, spot }).ToList();
                var sortedCbvZipped = zippedCbv.OrderBy(x =>
                {
                    int oneIndex = x.col.IndexOf(1.0);
                    return oneIndex >= 0 ? oneIndex : x.col.Count;
                }).ToList();

                var sortedBasicVars = sortedCbvZipped.Select(x => x.col).ToList();
                basicVarSpots = sortedCbvZipped.Select(x => x.spot).ToList();

                // Populate matrices
                var cbv = new List<double>();
                for (int i = 0; i < basicVarSpots.Count; i++)
                {
                    cbv.Add(-tableaus[0][0][basicVarSpots[i]]);
                }

                if (isConsoleOutput)
                {
                    OutputWriter.WriteLine($"Basic variable spots: [{string.Join(", ", basicVarSpots)}]");
                }

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

                if (isConsoleOutput)
                {
                    PrintMatrix(matrixCbv, "cbv");
                    PrintMatrix(matrixB, "B");
                    PrintMatrix(matrixBNegOne, "B^-1");
                    PrintMatrix(matrixCbvNegOne, "cbvB^-1");
                }

                // Work with the final tableau directly
                var firstTab = tableaus[0];

                // Get the z values of the new changing table
                var changingZRow = new List<double>();
                for (int j = 0; j < firstTab[firstTab.Count - 1].Count - 1; j++) // skip RHS column
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

                // Get the rhs optimal value
                var rhsCol = new List<double>();
                for (int i = 1; i < firstTab.Count; i++)
                {
                    rhsCol.Add(firstTab[i][firstTab[i].Count - 1]);
                }
                var rhsRow = new List<List<double>> { rhsCol };
                var rhsOptimal = MatMultiply(rhsRow, matrixCbvNegOne);
                double changingOptimal = rhsOptimal[0][0];

                // Get the b values of the new changing table
                var changingBvRows = new List<List<double>>();
                for (int j = 0; j < firstTab[firstTab.Count - 1].Count; j++) // all columns, including RHS
                {
                    var col = new List<double>();
                    for (int i = 1; i < firstTab.Count; i++)
                    {
                        col.Add(firstTab[i][j]);
                    }
                    var row = new List<List<double>> { col }; // make 1×n
                    var product = MatMultiply(row, matrixBNegOne); // (1×n) @ (n×n)
                    changingBvRows.Add(product[0]); // flatten row
                }

                // Transpose to match tableau format
                var transposeChangingB = MatTranspose(changingBvRows);

                // Rebuild changing table from final tableau
                var revisedTab = firstTab.Select(row => new List<double>(row)).ToList();

                changingZRow.Add(changingOptimal);
                revisedTab[0] = changingZRow;

                // Fill in rows under Z row
                for (int i = 0; i < revisedTab.Count - 1; i++)
                {
                    for (int j = 0; j < revisedTab[i].Count; j++)
                    {
                        revisedTab[i + 1][j] = transposeChangingB[i][j];
                    }
                }

                OutputWriter.WriteLine("Initial Table\n");
                foreach (string header in headerStr)
                {
                    OutputWriter.Write($"{header,-15} ");
                }
                OutputWriter.WriteLine();

                for (int i = 0; i < firstTab.Count; i++)
                {
                    for (int j = 0; j < firstTab[i].Count; j++)
                    {
                        OutputWriter.Write($"{firstTab[i][j],-15:F2} ");
                    }
                    OutputWriter.WriteLine();
                }

                OutputWriter.WriteLine("\nOptimal Changing Table\n");
                foreach (string header in headerStr)
                {
                    OutputWriter.Write($"{header,-15} ");
                }
                OutputWriter.WriteLine();

                for (int i = 0; i < revisedTab.Count; i++)
                {
                    for (int j = 0; j < revisedTab[i].Count; j++)
                    {
                        OutputWriter.Write($"{revisedTab[i][j],-15:F2} ");
                    }
                    OutputWriter.WriteLine();
                }

                // Extract solution from the optimal tableau and transform back
                var transformedSolution = ExtractSolution(revisedTab, headerStr);
                var originalSolution = TransformSolutionBack(transformedSolution);

                if (isConsoleOutput)
                {
                    OutputWriter.WriteLine("\nTransformed Solution:");
                    for (int i = 0; i < transformedSolution.Count; i++)
                    {
                        OutputWriter.WriteLine($"x'{i + 1} = {transformedSolution[i]:F4}");
                    }

                    OutputWriter.WriteLine("\nOriginal Solution (after back-transformation):");
                    for (int i = 0; i < originalSolution.Count; i++)
                    {
                        OutputWriter.WriteLine($"x{i + 1} = {originalSolution[i]:F4}");
                    }

                    OutputWriter.WriteLine($"\nOptimal objective value: {changingOptimal:F4}");

                    // Verify variable sign restrictions
                    VerifySignRestrictions(originalSolution, varSigns);
                }
            }

            private List<double> ExtractSolution(List<List<double>> tableau, List<string> headers)
            {
                var solution = new List<double>();

                // Find decision variables (not slack/surplus variables)
                for (int varIndex = 0; varIndex < headers.Count - 1; varIndex++) // -1 to skip RHS
                {
                    if (headers[varIndex] == "Z") continue;

                    // Check if this variable is basic
                    bool isBasic = false;
                    double value = 0;

                    for (int row = 1; row < tableau.Count; row++) // Skip Z row
                    {
                        if (Math.Abs(tableau[row][varIndex] - 1.0) < 1e-10)
                        {
                            // Check if this is the only non-zero in the column
                            bool isUnit = true;
                            for (int checkRow = 0; checkRow < tableau.Count; checkRow++)
                            {
                                if (checkRow != row && Math.Abs(tableau[checkRow][varIndex]) > 1e-10)
                                {
                                    isUnit = false;
                                    break;
                                }
                            }

                            if (isUnit)
                            {
                                isBasic = true;
                                value = tableau[row][tableau[row].Count - 1]; // RHS value
                                break;
                            }
                        }
                    }

                    solution.Add(isBasic ? value : 0);
                }

                return solution;
            }

            private void VerifySignRestrictions(List<double> solution, List<VariableSignType> varSigns)
            {
                OutputWriter.WriteLine("\nVerifying Variable Sign Restrictions:");
                OutputWriter.WriteLine("====================================");

                bool allValid = true;

                for (int i = 0; i < Math.Min(solution.Count, varSigns.Count); i++)
                {
                    double value = solution[i];
                    var signType = varSigns[i];
                    bool isValid = true;
                    string status = "✓ Valid";

                    switch (signType)
                    {
                        case VariableSignType.Positive:
                            if (value < -1e-10)
                            {
                                isValid = false;
                                status = "✗ Invalid (should be >= 0)";
                            }
                            break;

                        case VariableSignType.Negative:
                            if (value > 1e-10)
                            {
                                isValid = false;
                                status = "✗ Invalid (should be <= 0)";
                            }
                            break;

                        case VariableSignType.Unrestricted:
                            // Any value is valid for unrestricted variables
                            break;

                        case VariableSignType.Integer:
                            if (Math.Abs(value - Math.Round(value)) > 1e-10)
                            {
                                status = "Warning (should be integer)";
                            }
                            break;

                        case VariableSignType.Binary:
                            if (Math.Abs(value) > 1e-10 && Math.Abs(value - 1.0) > 1e-10)
                            {
                                status = "Warning (should be 0 or 1)";
                            }
                            break;
                    }

                    if (!isValid) allValid = false;

                    OutputWriter.WriteLine($"x{i + 1} = {value:F4} ({signType}) - {status}");
                }

                OutputWriter.WriteLine($"\nOverall: {(allValid ? "All constraints satisfied" : "Some constraints violated")}");
            }

            public void RunRevisedPrimalSimplex(List<double> objFuncPassed, List<List<double>> constraintsPassed, bool isMin, List<VariableSignType> varSigns = null)
            {
                objFunc = objFuncPassed.ToList();
                constraints = constraintsPassed.Select(x => x.ToList()).ToList();

                var simplex = new RevisedPrimalSimplex(isConsoleOutput: true);

                OutputWriter.WriteLine("=============================================================");
                OutputWriter.WriteLine($"Objective Function: Maximize {string.Join(" + ", objFunc.Select((c, i) => $"{c}x{i + 1}"))}");
                OutputWriter.WriteLine("\nOriginal Constraints:");

                for (int i = 0; i < constraints.Count; i++)
                {
                    var constraintTerms = new List<string>();
                    for (int j = 0; j < constraints[i].Count - 2; j++)
                    {
                        if (constraints[i][j] != 0)
                        {
                            string sign = constraints[i][j] > 0 && constraintTerms.Count > 0 ? "+" : "";
                            constraintTerms.Add($"{sign}{constraints[i][j]}x{j + 1}");
                        }
                    }
                    string constraintStr = string.Join(" ", constraintTerms);
                    string operator_str = constraints[i][constraints[i].Count - 1] == 0 ? "<=" : ">=";
                    OutputWriter.WriteLine($"  {constraintStr} {operator_str} {constraints[i][constraints[i].Count - 2]}");
                }

                OutputWriter.WriteLine("\n" + new string('=', 65));
                CanonicalFormBuilder.BuildCanonicalForm(objFunc, constraints, isMin);

                OutputWriter.WriteLine("\n" + new string('=', 65));
                DoPreliminaries(objFunc, constraints, isMin, varSigns);
            }
        }
    }
}






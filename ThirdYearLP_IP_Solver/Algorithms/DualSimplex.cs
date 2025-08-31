using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{


    public class DualSimplex
    {
        private bool isConsoleOutput;
        private List<int> IMPivotCols;
        private List<int> IMPivotRows;
        private List<string> IMHeaderRow;

        private List<int> phases;

        public DualSimplex(bool isConsoleOutput = false)
        {
            this.isConsoleOutput = isConsoleOutput;
            IMPivotCols = new List<int>();
            IMPivotRows = new List<int>();
            IMHeaderRow = new List<string>();
            phases = new List<int>();
        }

        public List<List<double>> DoFormulationOperation(List<double> objFunc, List<List<double>> constraints)
        {
            int excessCount = 0;
            int slackCount = 0;

            foreach (var constraint in constraints)
            {
                if (constraint.Last() == 1)
                    excessCount++;
                else
                    slackCount++;
            }

            for (int i = 0; i < constraints.Count; i++)
            {
                if (constraints[i].Last() == 1)
                {
                    for (int j = 0; j < constraints[i].Count; j++)
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
            int imCtr = 1;
            for (int i = 0; i < objFunc.Count; i++)
            {
                IMHeaderRow.Add($"x{imCtr++}");
            }

            imCtr = 1;
            if (excessCount > 0)
            {
                for (int i = 0; i < excessCount; i++)
                {
                    IMHeaderRow.Add($"e{imCtr++}");
                }
            }

            if (slackCount > 0)
            {
                for (int i = 0; i < slackCount; i++)
                {
                    IMHeaderRow.Add($"s{imCtr++}");
                }
            }

            IMHeaderRow.Add("rhs");

            int tableSizeW = excessCount + slackCount + 1 + objFunc.Count;
            var opTable = new List<List<double>>();
            for (int i = 0; i < tableSizeH; i++)
            {
                opTable.Add(new List<double>(new double[tableSizeW]));
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
                opTable[i + 1][tableSizeW - 1] = constraints[i].Last();
            }

            for (int i = 1; i < opTable.Count; i++)
            {
                for (int j = objFunc.Count; j < opTable[i].Count - 1; j++)
                {
                    opTable[i][i + objFunc.Count - 1] = 1;
                }
            }

            return opTable;
        }

        public (List<List<double>> newTab, List<double> thetaRow) DoDualPivotOperation(List<List<double>> tab)
        {
            var thetaRow = new List<double>();
            var rhs = tab.Select(row => row.Last()).ToList();
            var rhsNeg = rhs.Where(x => x < 0).ToList();
            if (!rhsNeg.Any()) return (tab, null);

            double minRhsNum = rhsNeg.Min();
            int pivotRow = rhs.IndexOf(minRhsNum);
            var negPivotCols = new List<double>();

            for (int i = 0; i < tab[pivotRow].Count - 1; i++)
            {
                negPivotCols.Add(tab[pivotRow][i] < 0 ? i : double.PositiveInfinity);
            }

            var dualPivotThetas = new List<double>();
            for (int i = 0; i < negPivotCols.Count; i++)
            {
                if (negPivotCols[i] != double.PositiveInfinity)
                {
                    int pivotColIndex = (int)negPivotCols[i];
                    dualPivotThetas.Add(Math.Abs(tab[0][pivotColIndex] / tab[pivotRow][pivotColIndex]));
                }
                else
                {
                    dualPivotThetas.Add(double.PositiveInfinity);
                }
            }

            thetaRow = dualPivotThetas.ToList();
            double smallestPosPivotTheta = dualPivotThetas.All(x => x == 0 || x == double.PositiveInfinity)
                ? 0
                : dualPivotThetas.Where(x => x > 0).DefaultIfEmpty(double.PositiveInfinity).Min();

            int rowIndex = pivotRow;
            int colIndex;
            try
            {
                colIndex = dualPivotThetas.IndexOf(smallestPosPivotTheta);
            }
            catch
            {
                return (tab, null);
            }

            var oldTab = tab.Select(row => row.ToList()).ToList();
            var newTab = oldTab.Select(row => new List<double>(new double[row.Count])).ToList();

            double divNumber = -1;
            try
            {
                divNumber = tab[rowIndex][colIndex];
            }
            catch
            {

                return (tab, null);
            }

            for (int j = 0; j < oldTab[rowIndex].Count; j++)
            {
                newTab[rowIndex][j] = oldTab[rowIndex][j] / divNumber;
                if (newTab[rowIndex][j] == -0.0) newTab[rowIndex][j] = 0.0;
            }

            var pivotMathRow = newTab[rowIndex].ToList();

            for (int i = 0; i < oldTab.Count; i++)
            {
                if (i == rowIndex) continue;
                for (int j = 0; j < oldTab[i].Count; j++)
                {
                    double mathItem = oldTab[i][j] - (oldTab[i][colIndex] * newTab[rowIndex][j]);
                    newTab[i][j] = mathItem;
                }
            }

            newTab[rowIndex] = pivotMathRow;

            if (isConsoleOutput)
            {
                OutputWriter.WriteLine($"the pivot col in Dual is {colIndex + 1} and the pivot row is {rowIndex + 1}");
            }

            IMPivotCols.Add(colIndex);
            IMPivotRows.Add(rowIndex);

            return (newTab, thetaRow);
        }

        public (List<List<double>> operationTab, List<double> thetasCol) DoPrimalPivotOperation(List<List<double>> tab, bool isMin)
        {
            var thetasCol = new List<double>();
            var testRow = tab[0].Take(tab[0].Count - 1).ToList();
            double largestNegativeNumber;

            try
            {
                largestNegativeNumber = isMin
                    ? testRow.Where(num => num > 0 && num != 0).Min()
                    : testRow.Where(num => num < 0 && num != 0).Min();
            }
            catch
            {
                return (null, null);
            }

            int colIndex = tab[0].IndexOf(largestNegativeNumber);
            var thetas = new List<double>();
            for (int i = 1; i < tab.Count; i++)
            {
                thetas.Add(tab[i][colIndex] != 0 ? tab[i].Last() / tab[i][colIndex] : double.PositiveInfinity);
            }

            thetasCol = thetas.ToList();
            bool allNegativeThetas = thetas.All(num => num < 0);

            if (allNegativeThetas)
                return (null, null);

            double minTheta;
            if (!thetas.Any(num => num > 0 && num != double.PositiveInfinity))
            {
                if (thetas.Contains(0))
                    minTheta = 0.0;
                else
                    return (null, null);
            }
            else
            {
                minTheta = thetas.Where(x => x > 0 && x != double.PositiveInfinity).Min();
            }

            if (minTheta == double.PositiveInfinity && !thetas.Contains(0))
                return (null, null);

            int rowIndex = thetas.IndexOf(minTheta) + 1;
            double divNumber = tab[rowIndex][colIndex];

            if (divNumber == 0)
                return (null, null);

            var operationTab = tab.Select(row => new List<double>(new double[row.Count])).ToList();

            for (int j = 0; j < tab[rowIndex].Count; j++)
            {
                operationTab[rowIndex][j] = tab[rowIndex][j] / divNumber;
                if (operationTab[rowIndex][j] == -0.0) operationTab[rowIndex][j] = 0.0;
            }

            for (int i = 0; i < tab.Count; i++)
            {
                if (i == rowIndex) continue;
                for (int j = 0; j < tab[i].Count; j++)
                {
                    double mathItem = tab[i][j] - (tab[i][colIndex] * operationTab[rowIndex][j]);
                    operationTab[i][j] = mathItem;
                }
            }

            if (isConsoleOutput)
            {
                OutputWriter.WriteLine($"the pivot col in primal is {colIndex + 1} and the pivot row is {rowIndex + 1}");
            }

            IMPivotCols.Add(colIndex);
            IMPivotRows.Add(rowIndex);

            return (operationTab, thetasCol);
        }

        public (List<List<double>> tab, bool isMin, int amtOfE, int amtOfS, int lenObj) GetInput(List<double> objFunc, List<List<double>> constraints, bool isMin)
        {
            int amtOfE = constraints.Count(c => c.Last() == 1 || c.Last() == 2);
            int amtOfS = constraints.Count(c => c.Last() != 1 && c.Last() != 2);
            var tab = DoFormulationOperation(objFunc, constraints);
            return (tab, isMin, amtOfE, amtOfS, objFunc.Count);
        }

        public (List<List<List<double>>> tableaus, List<double> changingVars, double? optimalSolution, List<int> pivotCols, List<int> pivotRows, List<string> headerRow) DoDualSimplex(List<double> objFunc, List<List<double>> constraints, bool isMin, List<List<double>> tabOverride = null)
        {
            var thetaCols = new List<List<double>>();
            var tableaus = new List<List<List<double>>>();
            var (tab, isMinLocal, amtOfE, amtOfS, lenObj) = GetInput(objFunc, constraints, isMin);

            if (tabOverride != null)
            {
                tab = tabOverride;
                IMPivotCols = new List<int>();
                IMPivotRows = new List<int>();
                IMHeaderRow.RemoveAt(IMHeaderRow.Count - 1);
            }

            tableaus.Add(tab);

            while (true)
            {
                foreach (var items in tableaus.Last())
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] == -0.0) items[i] = 0.0;
                    }
                }

                var rhsTest = tableaus.Last().Select(row => row.Last()).ToList();
                const double epsilon = 1e-9;
                bool allRhsPositive = rhsTest.All(num => num >= -epsilon);

                if (allRhsPositive)
                    break;

                var (newTab, thetaRow) = DoDualPivotOperation(tableaus.Last());

                if (thetaRow == null)
                {
                    if (tabOverride == null)
                    {
                        if (isConsoleOutput) OutputWriter.WriteLine("\nNo Optimal Solution Found");
                        return (tableaus, null, null, null, null, null);
                    }
                    return (tableaus, null, null, null, null, null);
                }

                foreach (var items in newTab)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] == -0.0) items[i] = 0.0;
                    }
                }

                tableaus.Add(newTab);
                phases.Add(0);
            }

            var objFuncTest = tableaus.Last()[0].Take(tableaus.Last()[0].Count - 1).ToList();
            bool allObjFuncPositive = isMinLocal
                ? objFuncTest.All(num => num <= 0)
                : objFuncTest.All(num => num >= 0);

            if (!allObjFuncPositive)
            {
                while (true)
                {
                    foreach (var items in tableaus.Last())
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (items[i] == -0.0) items[i] = 0.0;
                        }
                    }

                    if (tableaus.Last() == null)
                    {
                        if (isConsoleOutput) OutputWriter.WriteLine("\nNo Optimal Solution Found");
                        break;
                    }

                    objFuncTest = tableaus.Last()[0].Take(tableaus.Last()[0].Count - 1).ToList();
                    allObjFuncPositive = isMinLocal
                        ? objFuncTest.All(num => num <= 0)
                        : objFuncTest.All(num => num >= 0);

                    if (allObjFuncPositive)
                        break;

                    var (newTab, thetaCol) = DoPrimalPivotOperation(tableaus.Last(), isMinLocal);

                    if (thetaCol == null && tab == null)
                        break;

                    try
                    {
                        thetaCols.Add(thetaCol.ToList());
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    tableaus.Add(newTab);
                    phases.Add(1);
                }


                var rhsTest = tableaus.Last().Select(row => row.Last()).ToList();
                bool allRhsPositive = rhsTest.All(num => num >= 0);

                if (!allRhsPositive)
                {
                    tableaus.RemoveAt(tableaus.Count - 1);
                    IMPivotCols.RemoveAt(IMPivotCols.Count - 1);
                    IMPivotRows.RemoveAt(IMPivotRows.Count - 1);
                }
            }

            if (isConsoleOutput)
            {
                OutputWriter.WriteLine("\nOptimal Solution Found");
                if (tableaus.Last() != null)
                {
                    //Logger.WriteLine(tableaus.Last()[0].Last());
                    OutputWriter.WriteLine();
                }
                else
                {
                    OutputWriter.WriteLine("\nNo Optimal Solution Found");
                }
            }

            var xVars = Enumerable.Range(1, lenObj).Select(i => $"x{i}").ToList();
            int amtOfX = 0, amtOfSlack = 0, amtOfExcess = 0;
            var topRow = new List<string>();
            int topRowSize = lenObj + amtOfE + amtOfS;

            for (int i = 0; i < lenObj; i++)
            {
                if (amtOfX < lenObj)
                {
                    topRow.Add(xVars[amtOfX++]);
                }
            }

            for (int i = 0; i < amtOfE; i++)
            {
                if (amtOfSlack < amtOfE)
                {
                    topRow.Add($"e{amtOfExcess + 1}");
                    amtOfExcess++;
                }
            }

            for (int i = 0; i < amtOfS; i++)
            {
                if (amtOfExcess < amtOfS)
                {
                    topRow.Add($"s{amtOfSlack + 1}");
                    amtOfSlack++;
                }
            }

            topRow.Add("Rhs");

            if (isConsoleOutput)
            {
                for (int i = 0; i < tableaus.Count; i++)
                {
                    OutputWriter.WriteLine($"Tableau {i + 1}");
                    OutputWriter.WriteLine(string.Join(" ", topRow.Select(val => $"{val,10}")));
                    foreach (var row in tableaus[i])
                    {
                        OutputWriter.WriteLine(string.Join(" ", row.Select(val => $"{val,10:F3}")));
                    }
                    OutputWriter.WriteLine();
                }
            }

            var tSCVars = new List<List<double>>();
            for (int k = 0; k < lenObj; k++)
            {
                var tCVars = tableaus.Last().Select(row => row[k]).ToList();
                tSCVars.Add(tCVars.Count(num => num != 0) == 1 ? tCVars : null);
            }

            var changingVars = new List<double>();
            try
            {
                changingVars = new List<double>();
                for (int i = 0; i < tSCVars.Count; i++)
                {
                    if (tSCVars[i] != null)
                    {
                        tSCVars[i] = tSCVars[i].Select(Math.Abs).ToList();

                        changingVars.Add(tableaus.Last()[tSCVars[i].IndexOf(1.0)].Last());
                    }
                    else
                    {
                        changingVars.Add(0);
                    }
                }
            }
            catch
            {
                changingVars = new List<double>();
            }

            if (isConsoleOutput)
            {
                OutputWriter.WriteLine();
                OutputWriter.WriteLine(string.Join(" ", changingVars));
                OutputWriter.WriteLine();
                //Logger.WriteLine(tableaus.Last()[0].Last());
            }

            double optimalSolution = tableaus.Last()[0].Last();
            return (tableaus, changingVars, optimalSolution, IMPivotCols, IMPivotRows, IMHeaderRow);
        }
    }
}//fr


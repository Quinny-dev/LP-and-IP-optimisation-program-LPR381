using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    public class CuttingPlane
    {
        private double[,] tableau;
        private string[] columnNames;

        public CuttingPlane(double[,] tableau, string[] columnNames)
        {
            this.tableau = tableau;
            this.columnNames = columnNames;
        }

        public void RunCuttingPlane()
        {
            Console.WriteLine("\nCurrent Tableau (for reference):");
            PrintTableau();

            // Step 1: Find fractional RHS rows
            List<(int rowIndex, double frac)> fractionalRows = new List<(int, double)>();
            for (int i = 1; i < tableau.GetLength(0); i++)
            {
                double rhs = tableau[i, tableau.GetLength(1) - 1];
                double fracPart = rhs - Math.Floor(rhs);
                if (fracPart > 1e-9)
                    fractionalRows.Add((i, fracPart));
            }

            if (fractionalRows.Count == 0)
            {
                Console.WriteLine("No fractional RHS → no cut needed.");
                return;
            }

            // Step 2: Choose row closest to 0.5
            (int chosenRow, double chosenFrac) = ChooseRow(fractionalRows);
            Console.WriteLine($"\nChosen row for cut: Constraint {chosenRow} (fractional RHS = {chosenFrac:F2})");

            // Step 3: Generate MIR cut step by step
            GenerateMIRCut(chosenRow);
        }

        private (int, double) ChooseRow(List<(int rowIndex, double frac)> fractionalRows)
        {
            double bestDist = double.MaxValue;
            int chosenRow = -1;
            double chosenFrac = 0;

            foreach (var (rowIndex, frac) in fractionalRows)
            {
                double dist = Math.Abs(frac - 0.5);
                if (dist < bestDist || (Math.Abs(dist - bestDist) < 1e-9 && rowIndex < chosenRow))
                {
                    bestDist = dist;
                    chosenRow = rowIndex;
                    chosenFrac = frac;
                }
            }
            return (chosenRow, chosenFrac);
        }

        private void GenerateMIRCut(int rowIndex)
        {
            int cols = tableau.GetLength(1);
            List<string> lhsFractions = new List<string>();
            double rhsFraction = tableau[rowIndex, cols - 1] - Math.Floor(tableau[rowIndex, cols - 1]);
            double rhsConstant = 0; // number without variable

            Console.WriteLine("\nStep 1 — Split each coefficient into integer + fractional parts:");
            for (int j = 0; j < cols - 1; j++)
            {
                double val = tableau[rowIndex, j];
                double intPart = Math.Floor(val);
                double fracPart = val - intPart;
                Console.WriteLine($"{columnNames[j]}: {val} = {intPart} + {fracPart:F2}");
                if (Math.Abs(fracPart) > 1e-9)
                {
                    if (columnNames[j].StartsWith("x") || columnNames[j].StartsWith("s") || columnNames[j].StartsWith("e"))
                        lhsFractions.Add($"{fracPart:F2}{columnNames[j]}");
                    else
                        rhsConstant += fracPart;
                }
            }
            Console.WriteLine($"RHS: {tableau[rowIndex, cols - 1]} = {Math.Floor(tableau[rowIndex, cols - 1])} + {rhsFraction:F2}");

            Console.WriteLine("\nStep 2 — Cancel LHS integers (remove them).");

            Console.WriteLine("\nStep 3 — Remaining fractional parts as LHS (<= 0):");
            List<string> finalLHS = new List<string>();
            foreach (var frac in lhsFractions)
                finalLHS.Add($"-{frac}");

            string lhsExpression = string.Join(" + ", finalLHS);
            double rhsValue = -rhsConstant - rhsFraction; // move constants to RHS
            Console.WriteLine($"{lhsExpression} <= {rhsValue:F2}");

            Console.WriteLine("\nFinal MIR Cut:");
            Console.WriteLine($"{lhsExpression} <= {rhsValue:F2}");
        }

        private void PrintTableau()
        {
            Console.Write("        ");
            foreach (var col in columnNames)
                Console.Write($"{col,8}");
            Console.WriteLine();

            for (int i = 0; i < tableau.GetLength(0); i++)
            {
                string rowLabel = (i == 0) ? "Z" : $"C{i}";
                Console.Write($"{rowLabel,-6} ");
                for (int j = 0; j < tableau.GetLength(1); j++)
                    Console.Write($"{tableau[i, j],8:F2}");
                Console.WriteLine();
            }
        }
    }
}

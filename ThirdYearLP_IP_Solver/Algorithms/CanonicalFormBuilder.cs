using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    static class CanonicalFormBuilder
    {
        public static List<List<string>> BuildCanonicalForm(List<double> objective, List<List<double>> constraints, bool isMin = false)
        {
            var canonicalRows = new List<List<string>>();
            int slackCount = 0;
            int excessCount = 0;

            // Objective row
            var objRow = new List<string> { "(z)" };
            for (int i = 0; i < objective.Count; i++)
            {
                double coef = objective[i];

                // If minimization, flip sign
                if (isMin)
                    coef = -coef;

                // Always negate for canonical form
                objRow.Add($"- {coef}x{i + 1}");
            }
            objRow.Add("= 0");
            canonicalRows.Add(objRow);

            // Constraint rows
            foreach (var row in constraints)
            {
                var coefs = row.Take(row.Count - 2).ToList();
                double rhs = row[row.Count - 2];
                int ctype = (int)row[row.Count - 1];

                var terms = new List<string>();

                if (ctype == 0) // <= slack
                {
                    slackCount++;
                    for (int i = 0; i < coefs.Count; i++)
                    {
                        if (coefs[i] != 0)
                            terms.Add($"{coefs[i]}x{i + 1}");
                    }
                    terms.Add($"s{slackCount}");
                    terms.Add($"= {rhs}");
                }
                else // >= excess -> multiply by -1
                {
                    excessCount++;
                    coefs = coefs.Select(c => -c).ToList();
                    rhs = -rhs;

                    for (int i = 0; i < coefs.Count; i++)
                    {
                        if (coefs[i] != 0)
                            terms.Add($"{coefs[i]}x{i + 1}");
                    }
                    terms.Add($"e{excessCount}");
                    terms.Add($"= {rhs}");
                }

                canonicalRows.Add(terms);
            }

            OutputWriter.WriteLine("Canonical Form:");
            OutputWriter.WriteLine(string.Format("{0} {1}", isMin ? "Minimize" : "Maximize", ""));
            foreach (var row in canonicalRows)
            {
                OutputWriter.WriteLine(string.Join(" + ", row));
            }

            return canonicalRows;
        }
    }
}


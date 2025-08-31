using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ThirdYearLP_IP_Solver.HelperClasses
{
    internal class InputParser
    {
        private string filePath;

        public InputParser(string path)
        {
            filePath = path;
        }

        public string GetLPTableAsString()
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath)
                                   .Where(l => !string.IsNullOrWhiteSpace(l))
                                   .ToArray();

                if (lines.Length < 2)
                {
                    throw new ArgumentException("Invalid file format. Expected at least 2 lines.");
                }

                // Parse the input 
                var objectiveInfo = ParseObjectiveLine(lines[0]);
                var constraintInfoList = new List<(double[] coefficients, string inequality, double rhs)>();

                // Parse all constraint lines (from line 1 onwards)
                for (int i = 1; i < lines.Length; i++)
                {
                    // Skip binary/integer indicator lines
                    if (lines[i].Trim().ToLower().StartsWith("bin"))
                        continue;

                    var constraintInfo = ParseConstraintLine(lines[i]);
                    constraintInfoList.Add(constraintInfo);
                }

                // Build the LP table
                return BuildLPTable(objectiveInfo, constraintInfoList);
            }
            catch (Exception e)
            {
                return $"Error parsing LP file: {e.Message}";
            }
        }

        private (string type, double[] coefficients) ParseObjectiveLine(string line)
        {
            var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string objectiveType = parts[0].ToLower(); // "max" or "min"
            List<double> coefficients = new List<double>();

            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("+") || parts[i].StartsWith("-") || char.IsDigit(parts[i][0]))
                {
                    coefficients.Add(double.Parse(parts[i]));
                }
            }

            return (objectiveType, coefficients.ToArray());
        }

        private (double[] coefficients, string inequality, double rhs) ParseConstraintLine(string line)
        {
            var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            List<double> coefficients = new List<double>();
            string inequality = "";
            double rhs = 0;

            int i = 0;
            // Parse coefficients
            while (i < parts.Length && (parts[i].StartsWith("+") || parts[i].StartsWith("-") || char.IsDigit(parts[i][0])))
            {
                coefficients.Add(double.Parse(parts[i]));
                i++;
            }

            // Parse inequality
            if (i < parts.Length && (parts[i].Contains("<=") || parts[i].Contains(">=") || parts[i] == "="))
            {
                if (parts[i].StartsWith("<="))
                {
                    inequality = "<=";
                    // Check if RHS is attached to the inequality symbol
                    if (parts[i].Length > 2)
                    {
                        string rhsStr = parts[i].Substring(2);
                        rhs = double.Parse(rhsStr);
                    }
                    else
                    {
                        i++;
                        if (i < parts.Length)
                        {
                            rhs = double.Parse(parts[i]);
                        }
                    }
                }
                else if(inequality == ">=")
                {
                    // Flip coefficients and rhs for dual form
                    for (int j = 0; j < coefficients.Count; j++)
                        coefficients[j] = -coefficients[j];

                    rhs = -rhs;

                    // Mark as needing an "e" variable
                    inequality = "dual"; // custom marker so we know to add +1 e-variable
                }                
                else if (parts[i] == "=")
                {
                    inequality = "=";
                    i++;
                    if (i < parts.Length)
                    {
                        rhs = double.Parse(parts[i]);
                    }
                }
            }

            return (coefficients.ToArray(), inequality, rhs);
        }

        private string BuildLPTable(
            (string type, double[] coefficients) objective,
            List<(double[] coefficients, string inequality, double rhs)> constraints)
        {
            StringBuilder table = new StringBuilder();

            // Determine the maximum number of decision variables
            int maxVariables = objective.coefficients.Length;
            foreach (var constraint in constraints)
            {
                maxVariables = Math.Max(maxVariables, constraint.coefficients.Length);
            }

            // Header
            table.AppendLine("=== LINEAR PROGRAMMING TABLE ===");
            table.AppendLine();

            // Objective Function
            table.AppendLine($"Objective: {objective.type.ToUpper()}");
            table.Append("Z = ");
            for (int i = 0; i < objective.coefficients.Length; i++)
            {
                if (i > 0 && objective.coefficients[i] >= 0)
                    table.Append(" + ");
                else if (objective.coefficients[i] < 0)
                    table.Append(" ");

                table.Append($"{objective.coefficients[i]}x{i + 1}");
            }
            table.AppendLine();
            table.AppendLine();

            // Constraints
            table.AppendLine("Subject to:");
            for (int constraintIndex = 0; constraintIndex < constraints.Count; constraintIndex++)
            {
                var constraint = constraints[constraintIndex];
                table.Append("  ");

                for (int i = 0; i < constraint.coefficients.Length; i++)
                {
                    if (i > 0 && constraint.coefficients[i] >= 0)
                        table.Append(" + ");
                    else if (constraint.coefficients[i] < 0)
                        table.Append(" ");

                    table.Append($"{constraint.coefficients[i]}x{i + 1}");
                }
                table.AppendLine($" {constraint.inequality} {constraint.rhs}");
            }
            table.AppendLine();

            // Count extra variables (slacks + dual e’s)
            int slackCount = constraints.Count(c => c.inequality == "<=");
            int eCount = constraints.Count(c => c.inequality == "dual");

            // Table headers
            for (int i = 0; i < slackCount; i++)
                table.Append($"s{i + 1}\t");
            for (int i = 0; i < eCount; i++)
                table.Append($"e{i + 1}\t");

            // MAX Initial Simplex Table 
            if (objective.type.ToLower() == "max")
            {
                table.AppendLine("Initial Simplex Table:");
                table.AppendLine("========================");

                // Table headers
                table.Append("Basic Var\t");
                for (int i = 0; i < maxVariables; i++)
                {
                    table.Append($"x{i + 1}\t");
                }

                // Add slack variable headers
                for (int i = 0; i < slackCount; i++)
                {
                    table.Append($"s{i + 1}\t");
                }

                table.AppendLine("RHS");

                // Constraint rows
                int slackIndex = 0;
                for (int constraintIndex = 0; constraintIndex < constraints.Count; constraintIndex++)
                {
                    var constraint = constraints[constraintIndex];

                    // Basic variable name
                    if (constraint.inequality == "<=")
                    {
                        table.Append($"s{slackIndex + 1}\t");
                    }
                    else
                    {
                        table.Append($"c{constraintIndex + 1}\t"); // For = or >= constraints
                    }

                    // Decision variable coefficients
                    for (int i = 0; i < maxVariables; i++)
                    {
                        if (i < constraint.coefficients.Length)
                        {
                            table.Append($"{constraint.coefficients[i]}\t");
                        }
                        else
                        {
                            table.Append("0\t"); // Pad with zeros if constraint has fewer variables
                        }
                    }

                    // Slack variable coefficients
                    for (int i = 0; i < slackCount; i++)
                    {
                        if (constraint.inequality == "<=" && i == slackIndex)
                        {
                            table.Append("1\t");
                        }
                        else
                        {
                            table.Append("0\t");
                        }
                    }

                    table.AppendLine($"{constraint.rhs}");


                    if (constraint.inequality == "<=")
                    {
                        slackIndex++;
                    }


                }

                // Objective row (Z row)
                table.Append("Z\t");

                // Negative objective coefficients for decision variables
                for (int i = 0; i < maxVariables; i++)
                {
                    if (i < objective.coefficients.Length)
                    {
                        table.Append($"{-objective.coefficients[i]}\t");
                    }
                    else
                    {
                        table.Append("0\t"); // Pad with zeros
                    }
                }

                // Zeros for slack variables in objective row
                for (int i = 0; i < slackCount; i++)
                {
                    table.Append("0\t");
                }

                table.AppendLine("0");
            }

            return table.ToString();
        }

        public LPProblem GetLPProblemData()
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath)
                                   .Where(l => !string.IsNullOrWhiteSpace(l))
                                   .ToArray();

                if (lines.Length < 2)
                {
                    throw new ArgumentException("Invalid file format. Expected at least 2 lines.");
                }

                var objectiveInfo = ParseObjectiveLine(lines[0]);
                var constraintsList = new List<Constraint>();

                // Parse all constraint lines
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Trim().ToLower().StartsWith("bin"))
                        continue;

                    var constraintInfo = ParseConstraintLine(lines[i]);
                    constraintsList.Add(new Constraint
                    {
                        Coefficients = constraintInfo.coefficients,
                        InequalityType = constraintInfo.inequality,
                        RHS = constraintInfo.rhs
                    });
                }

                return new LPProblem
                {
                    ObjectiveType = objectiveInfo.type,
                    ObjectiveCoefficients = objectiveInfo.coefficients,
                    Constraints = constraintsList,
                    VariableCount = objectiveInfo.coefficients.Length,
                    ConstraintCount = constraintsList.Count
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing LP file: {e.Message}");
            }
        }
    }

    // Helper classes to hold LP problem data
    public class Constraint
    {
        public double[] Coefficients { get; set; }
        public string InequalityType { get; set; } // "<=", ">=", or "="
        public double RHS { get; set; }
        public bool RequiresSlackVariable => InequalityType == "<=";
    }

    public class LPProblem
    {
        public string ObjectiveType { get; set; }
        public double[] ObjectiveCoefficients { get; set; }
        public List<Constraint> Constraints { get; set; }
        public int VariableCount { get; set; }
        public int ConstraintCount { get; set; }

        public int SlackVariableCount => Constraints.Count(c => c.RequiresSlackVariable);
        public bool HasSlackVariables => SlackVariableCount > 0;
    }
}
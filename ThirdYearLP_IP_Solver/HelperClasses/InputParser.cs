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

                // Parse the input (ignore third line with variable types)
                var objectiveInfo = ParseObjectiveLine(lines[0]);
                var constraintInfo = ParseConstraintLine(lines[1]);

                // Build the LP table
                return BuildLPTable(objectiveInfo, constraintInfo);
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
                else if (parts[i].StartsWith(">="))
                {
                    inequality = ">=";
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

        private string[] ParseVariableTypeLine(string line)
        {
            return line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string BuildLPTable(
            (string type, double[] coefficients) objective,
            (double[] coefficients, string inequality, double rhs) constraint)
        {
            StringBuilder table = new StringBuilder();

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
            table.AppendLine();

            // Check if we need slack variables
            bool hasSlackVariable = constraint.inequality == "<=";

            // MAX Initial Simplex Table 
            if (objective.type.ToLower() == "max")
            {
                table.AppendLine("Initial Simplex Table:");
                table.AppendLine("========================");

                // Table headers
                table.AppendLine("Basic Var\t");
                table.Append("\t");
                for (int i = 0; i < objective.coefficients.Length; i++)
                {
                    table.Append($"x{i + 1}\t");
                }

                if (hasSlackVariable)
                {
                    table.Append("s1\t");
                }

                table.Append("RHS");
                table.AppendLine();

                // Constraint row
                table.Append("s1\t");

                for (int i = 0; i < constraint.coefficients.Length; i++)
                {
                    table.Append($"{constraint.coefficients[i]}\t");
                }

                if (hasSlackVariable)
                {
                    table.Append($"1\t");
                }

                table.Append($"{constraint.rhs}");
                table.AppendLine();

                // Objective row (Z row)
                table.Append("Z\t");
                for (int i = 0; i < objective.coefficients.Length; i++)
                {
                    table.Append($"{-objective.coefficients[i]}\t"); 
                }

                if (hasSlackVariable)
                {
                    table.Append("0\t");
                }

                table.Append("0");
                table.AppendLine();
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

                var objectiveInfo = ParseObjectiveLine(lines[0]);
                var constraintInfo = ParseConstraintLine(lines[1]);

                return new LPProblem
                {
                    ObjectiveType = objectiveInfo.type,
                    ObjectiveCoefficients = objectiveInfo.coefficients,
                    ConstraintCoefficients = constraintInfo.coefficients,
                    ConstraintType = constraintInfo.inequality,
                    RHS = constraintInfo.rhs,
                    HasSlackVariable = constraintInfo.inequality == "<="
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing LP file: {e.Message}");
            }
        }
    }

    // Helper class to hold LP problem data
    public class LPProblem
    {
        public string ObjectiveType { get; set; }
        public double[] ObjectiveCoefficients { get; set; }
        public double[] ConstraintCoefficients { get; set; }
        public string ConstraintType { get; set; }
        public double RHS { get; set; }
        public bool HasSlackVariable { get; set; }
    }
}
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
            StringBuilder sb = new StringBuilder();
            try
            {
                string fileContents = File.ReadAllLines(filePath)
                                    .Where(l => !string.IsNullOrWhiteSpace(l)) // optional: skip empty lines
                                    .Aggregate((current, next) => current + Environment.NewLine + next);
                sb.AppendLine(fileContents);
            }
            catch (Exception e)
            {
                sb.AppendLine("Exception: " + e.Message);
            }

            return sb.ToString();
        }
    }
}
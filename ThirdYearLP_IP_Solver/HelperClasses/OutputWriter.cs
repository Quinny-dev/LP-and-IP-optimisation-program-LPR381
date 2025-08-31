using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.HelperClasses
{
    public class OutputWriter
    {
        private static StreamWriter writer;

        public static void Init(string filePath = "output.txt", bool append = false)
        {
            writer?.Dispose();
            writer = new StreamWriter(filePath, append);
            writer.AutoFlush = true;
        }

        public static void Close()
        {
            //writer?.Close();
            writer?.Dispose();
            writer = null;
        }

        public static void Write(string message)
        {
            writer.Write(message);
        }

        public static void WriteLine(string message = "")
        {
            writer.WriteLine(message);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class SimplexIndexResult
    {
        public Tuple<int, int> index;  // Pivot element coordinates
        public SimplexResult result;   // Current algorithm state

        public SimplexIndexResult(Tuple<int, int> index, SimplexResult result)
        {
            this.index = index;
            this.result = result;
        }
    }

    public enum SimplexResult
    {
        Unbounded,      // Problem has unbounded solution
        Found,          // Optimal solution found
        NotYetFound     // Algorithm still iterating
    }
}

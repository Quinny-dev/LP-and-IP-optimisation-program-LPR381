using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    public enum SimplexResult { Unbounded, Found, NotYetFound }
    internal class PrimalSimplex
    {
        private LPProblem _problem;

        public List<DataTable> Iterations { get; private set; }

        public PrimalSimplex(LPProblem problem)
        {
            _problem = problem;
            Iterations = new List<DataTable>();
        }
        public void Solve()
        {
            // TODO: Setup initial tableau from _problem
            DataTable initial = BuildTableau(_problem);
            Iterations.Add(initial);

            bool optimal = false;
            int iteration = 0;

            while (!optimal && iteration < 50) // safety cap
            {
                iteration++;

                // TODO: perform pivot calculations here
                DataTable newTableau = DoPivot(Iterations.Last());

                Iterations.Add(newTableau);

                // TODO: check optimality
                optimal = CheckOptimality(newTableau);
            }
        }

        private DataTable BuildTableau(LPProblem problem)
        {
            DataTable table = new DataTable();
            // reuse your BuildDataTable logic from MainForm
            return table;
        }

        private DataTable DoPivot(DataTable oldTable)
        {
            DataTable newTable = oldTable.Copy();
            // TODO: apply pivot operation
            return newTable;
        }

        private bool CheckOptimality(DataTable table)
        {
            // TODO: stop when all objective coefficients ≥ 0
            return true;
        }
    }
}

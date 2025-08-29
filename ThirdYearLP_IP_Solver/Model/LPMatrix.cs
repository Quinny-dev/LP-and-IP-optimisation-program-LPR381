using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;


namespace ThirdYearLP_IP_Solver.Model
{
    public sealed class LPMatrix
    {
        public Matrix<double> Table { get; private set; }
        //numeric table - the obj func and constraints
        public int ObjectiveRowIndex { get; private set; }
        public int RHSColumnIndex { get; private set; }
        public IReadOnlyList<string> ColumnLabels { get; private set; }

        public int RowCount => Table.RowCount;
        public int ColCount => Table.ColumnCount;

        public LPMatrix(Matrix<double> table, int objectiveRowIndex, int rhsColumnIndex, IReadOnlyList<string>? columnLabels = null, IReadOnlyList<string>? rowLabels = null) { 
        
        }

    }
}

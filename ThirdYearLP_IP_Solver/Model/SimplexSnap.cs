using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThirdYearLP_IP_Solver.Model
{
    public class SimplexSnap
    {
        public double[] b;          // Current RHS values
        public double[][] matrix;   // Current simplex tableau
        public double[] M;          // Big M method coefficients
        public double[] F;          // Objective function coefficients
        public int[] C;             // Current basis variables
        public double fValue;       // Current objective function value
        public double[] fVars;      // Original function variables
        public bool isMDone;        // Phase I completion flag
        public bool[] m;            // Artificial variable flags

        public SimplexSnap(double[] b, double[][] matrix, double[] M, double[] F,
                          int[] C, double[] fVars, bool isMDone, bool[] m)
        {
            this.b = Copy(b);
            this.matrix = Copy(matrix);
            this.M = Copy(M);
            this.F = Copy(F);
            this.C = Copy(C);
            this.isMDone = isMDone;
            this.m = Copy(m);
            this.fVars = Copy(fVars);

            // Calculate current objective function value
            fValue = 0;
            for (int i = 0; i < C.Length; i++)
            {
                fValue += fVars[C[i]] * b[i];
            }
        }

        // Deep copy methods to ensure snapshot independence
        T[] Copy<T>(T[] array)
        {
            T[] newArr = new T[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newArr[i] = array[i];
            }
            return newArr;
        }

        T[][] Copy<T>(T[][] matrix)
        {
            T[][] newMatr = new T[matrix.Length][];
            for (int i = 0; i < matrix.Length; i++)
            {
                newMatr[i] = new T[matrix.First().Length];
                for (int j = 0; j < matrix.First().Length; j++)
                {
                    newMatr[i][j] = matrix[i][j];
                }
            }
            return newMatr;
        }
    }
}
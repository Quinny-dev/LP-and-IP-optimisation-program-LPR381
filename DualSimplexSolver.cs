using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptiSolve
{
	/// <summary>
	/// Dual Simplex (Phase 1) + Primal Simplex (Phase 2) for maximization.
	///
	/// Matches Belgium Campus slide rules:
	/// - Canonical form: 
	///   >= : subtract excess, multiply full row by -1 so the excess enters as basic
	///   <= : add slack
	///   =  : duplicate as <= and >= then apply above
	/// - Dual pivoting while any RHS < 0:
	///   pick pivot row with most negative RHS;
	///   pivot column j among a_rj < 0 and z_j < 0 that minimizes θ = |z_j / a_rj|.
	/// - After RHS ≥ 0, finish with primal simplex until no negative reduced costs.
	///
	/// Prints tables like SimplexSolver (z row; constraint rows; optional θ).
	/// </summary>
	public class DualSimplexSolver
	{
		private readonly OutputWriter _out;
		private readonly string[]? _varNames;

		public DualSimplexSolver(OutputWriter writer)
		{
			_out = writer;
			_varNames = null;
		}

		public DualSimplexSolver(OutputWriter writer, string[] varNames)
		{
			_out = writer;
			_varNames = varNames;
		}

		// Compat overload
		public SolveResult Solve(LpModel model, bool _ignore) => Solve(model);

		public SolveResult Solve(LpModel model)
		{
			// Canonicalize exactly like your Primal Simplex (Big-M etc. already done upstream).
			var cf = CanonicalForm.FromModel(model);

			// Clone to avoid side-effects
			var A = (double[,])cf.A.Clone();  // m x n
			var b = (double[])cf.b.Clone();   // m
			var c = (double[])cf.c.Clone();   // n

			int m = A.GetLength(0);
			int n = A.GetLength(1);

			var names = (_varNames != null && _varNames.Length >= n)
				? _varNames.Take(n).ToArray()
				: Enumerable.Range(1, n).Select(k => $"x{k}").ToArray();

			// Try to detect a basic set (identity columns)
			var basic = GuessInitialBasis(A);

			// Build tableau T (z + m rows) x (n + 1) cols; last col is RHS
			double[,] T = new double[m + 1, n + 1];

			// z-row = -c | 0 (max)
			for (int j = 0; j < n; j++) T[0, j] = -c[j];
			T[0, n] = 0.0;

			// constraints
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++) T[i + 1, j] = A[i, j];
				T[i + 1, n] = b[i];
			}

			// Put tableau into canonical shape for the current basis
			CanonicalizeToBasis(T, basic);

			// Print initial tableau
			PrintTableau(T, names, "t-i", showRatiosForPivotCol: -1);

			// ---------------- PHASE 1: Dual Simplex ----------------
			int iter = 1;
			while (true)
			{
				int prow = ArgMostNegativeRhs(T);
				if (prow < 0) break; // No negatives in RHS → dual phase complete

				int pcol = ChooseDualPivotColumn(T, prow);
				if (pcol < 0)
				{
					_out.WriteLine("");
					_out.WriteLine("No valid entering column (a_rj < 0 with z_j < 0). Primal is infeasible (dual simplex stalled).");
					return BuildResultFromTableau(T, names, basic, SolveStatus.Infeasible,
						"Infeasible: Dual Simplex could not find an entering variable.");
				}

				string entering = names[pcol];
				string leaving = names[basic[prow - 1]];
				double theta = Math.Abs(T[0, pcol] / T[prow, pcol]);

				_out.WriteLine("");
				_out.WriteLine($"[Dual] Pivot: Entering = {entering}, Leaving = {leaving}  (row {prow}, col {pcol + 1}, θ={theta:F3})");

				Pivot(T, prow, pcol);
				basic[prow - 1] = pcol;

				iter++;
				// For display, we can show θ against the chosen col (like primal printer).
				PrintTableau(T, names, $"t-{iter}", showRatiosForPivotCol: pcol);
			}

			_out.WriteLine("");
			_out.WriteLine("Dual phase complete (no negative RHS). Switching to primal phase if needed.");

			// ---------------- PHASE 2: Primal Simplex ----------------
			while (true)
			{
				int pcol = ArgMostNegative(T);
				if (pcol < 0)
				{
					_out.WriteLine("");
					_out.WriteLine("No negative reduced costs in z-row → Optimal tableau.");
					break;
				}

				int prow = RatioTest(T, pcol);
				if (prow < 0)
				{
					_out.WriteLine("");
					_out.WriteLine("All entries in pivot column ≤ 0 → Unbounded.");
					return BuildResultFromTableau(T, names, basic, SolveStatus.Unbounded,
						"Unbounded during primal phase after dual feasibility.");
				}

				string entering = names[pcol];
				string leaving = names[basic[prow - 1]];
				_out.WriteLine("");
				_out.WriteLine($"[Primal] Pivot: Entering = {entering}, Leaving = {leaving}  (row {prow}, col {pcol + 1})");

				Pivot(T, prow, pcol);
				basic[prow - 1] = pcol;

				iter++;
				PrintTableau(T, names, $"t-{iter}", showRatiosForPivotCol: pcol);
			}

			// Extract optimal solution
			var result = BuildResultFromTableau(T, names, basic, SolveStatus.Optimal, "Dual+Primal Simplex optimal.");

			_out.StartSection("Dual Simplex Optimal Solution");
			_out.WriteLine($"Objective (Z) = {result.ObjectiveValue:F3}");
			foreach (var kv in result.VariableValues.OrderBy(k => k.Key))
			{
				int j = kv.Key;
				_out.WriteLine($"{names[j]} = {kv.Value:F3}");
			}

			return result;
		}

		// --------------------- helpers ----------------------

		private static int[] GuessInitialBasis(double[,] A)
		{
			int m = A.GetLength(0);
			int n = A.GetLength(1);
			var basis = new List<int>();

			for (int j = 0; j < n; j++)
			{
				int oneRow = -1; bool ok = true;
				for (int i = 0; i < m; i++)
				{
					double v = A[i, j];
					if (Math.Abs(v - 1.0) < 1e-9)
					{
						if (oneRow >= 0) { ok = false; break; }
						oneRow = i;
					}
					else if (Math.Abs(v) > 1e-9)
					{
						ok = false; break;
					}
				}
				if (ok && oneRow >= 0)
				{
					basis.Add(j);
					if (basis.Count == m) break;
				}
			}
			while (basis.Count < m) basis.Add(basis.Count);
			return basis.ToArray();
		}

		private static void CanonicalizeToBasis(double[,] T, int[] basic)
		{
			int m = T.GetLength(0) - 1;
			int n = T.GetLength(1) - 1;

			for (int r = 1; r <= m; r++)
			{
				int j = basic[r - 1];
				double piv = T[r, j];
				if (Math.Abs(piv) > 1e-12)
				{
					for (int c = 0; c <= n; c++) T[r, c] /= piv;
				}

				for (int rr = 0; rr <= m; rr++)
				{
					if (rr == r) continue;
					double f = T[rr, j];
					if (Math.Abs(f) < 1e-12) continue;
					for (int c = 0; c <= n; c++) T[rr, c] -= f * T[r, c];
				}
			}
		}

		// Primal: most negative in z row (exclude RHS)
		private static int ArgMostNegative(double[,] T)
		{
			int n = T.GetLength(1) - 1;
			int bestJ = -1;
			double bestVal = 0.0;
			for (int j = 0; j < n; j++)
			{
				double v = T[0, j];
				if (v < bestVal - 1e-12)
				{
					bestVal = v;
					bestJ = j;
				}
			}
			return bestJ;
		}

		// Dual: most negative RHS row (strictly < 0)
		private static int ArgMostNegativeRhs(double[,] T)
		{
			int m = T.GetLength(0) - 1;
			int n = T.GetLength(1) - 1;
			int bestRow = -1;
			double mostNeg = -1e-12; // strictly negative threshold
			for (int r = 1; r <= m; r++)
			{
				double rhs = T[r, n];
				if (rhs < mostNeg)
				{
					mostNeg = rhs;
					bestRow = r;
				}
			}
			return bestRow;
		}

		// Dual pivot column selector:
		// Among columns with a_rj < 0 and z_j < 0, minimize theta = |z_j / a_rj|.
		private static int ChooseDualPivotColumn(double[,] T, int prow)
		{
			int n = T.GetLength(1) - 1;
			int bestJ = -1;
			double bestTheta = double.PositiveInfinity;

			for (int j = 0; j < n; j++)
			{
				double a = T[prow, j];
				double zj = T[0, j];

				if (a < -1e-12 && zj < -1e-12)
				{
					double th = Math.Abs(zj / a);
					if (th < bestTheta - 1e-12 || (Math.Abs(th - bestTheta) < 1e-12 && j < bestJ))
					{
						bestTheta = th;
						bestJ = j;
					}
				}
			}
			return bestJ; // -1 ⇒ Infeasible (no valid entering)
		}

		// Primal ratio test: smallest positive RHS / a_ip
		private static int RatioTest(double[,] T, int pcol)
		{
			int m = T.GetLength(0) - 1;
			int n = T.GetLength(1) - 1;
			int bestRow = -1;
			double bestTheta = double.PositiveInfinity;

			for (int r = 1; r <= m; r++)
			{
				double a = T[r, pcol];
				if (a > 1e-12)
				{
					double rhs = T[r, n];
					double th = rhs / a;
					if (th < bestTheta - 1e-12 || (Math.Abs(th - bestTheta) < 1e-12 && r < bestRow))
					{
						bestTheta = th;
						bestRow = r;
					}
				}
			}
			return bestRow;
		}

		private static void Pivot(double[,] T, int prow, int pcol)
		{
			int n = T.GetLength(1) - 1;
			int m = T.GetLength(0) - 1;

			double piv = T[prow, pcol];
			if (Math.Abs(piv) < 1e-12) throw new InvalidOperationException("Zero pivot.");

			for (int j = 0; j <= n; j++) T[prow, j] /= piv;

			for (int i = 0; i <= m; i++)
			{
				if (i == prow) continue;
				double f = T[i, pcol];
				if (Math.Abs(f) < 1e-12) continue;
				for (int j = 0; j <= n; j++) T[i, j] -= f * T[prow, j];
			}
		}

		private void PrintTableau(double[,] T, string[] names, string header, int showRatiosForPivotCol)
		{
			int m = T.GetLength(0) - 1;
			int n = T.GetLength(1) - 1;

			_out.StartSection(header);

			var head = new List<string> { "t".PadRight(6) };
			head.AddRange(names.Take(n).Select(nm => nm.PadLeft(8)));
			head.Add("rhs".PadLeft(8));
			if (showRatiosForPivotCol >= 0) head.Add("θ".PadLeft(8));
			_out.WriteLine(string.Join("", head));

			// z-row
			var zline = new List<string> { "z".PadRight(6) };
			for (int j = 0; j < n; j++) zline.Add($"{T[0, j],8:F3}");
			zline.Add($"{T[0, n],8:F3}");
			_out.WriteLine(string.Join("", zline));

			// constraint rows
			for (int i = 1; i <= m; i++)
			{
				var row = new List<string> { $"{i}".PadRight(6) };
				for (int j = 0; j < n; j++) row.Add($"{T[i, j],8:F3}");
				row.Add($"{T[i, n],8:F3}");

				if (showRatiosForPivotCol >= 0)
				{
					double a = T[i, showRatiosForPivotCol];
					double rhs = T[i, n];
					string theta = (a > 1e-12) ? (rhs / a).ToString("F3") : "-";
					row.Add($"{theta,8}");
				}

				_out.WriteLine(string.Join("", row));
			}
		}

		private SolveResult BuildResultFromTableau(double[,] T, string[] names, int[] basic, SolveStatus status, string message)
		{
			int m = T.GetLength(0) - 1;
			int n = T.GetLength(1) - 1;

			var x = new double[n];
			for (int r = 1; r <= m; r++)
			{
				int j = basic[r - 1];
				if (j >= 0 && j < n) x[j] = T[r, n];
			}
			double z = T[0, n];

			var dict = new Dictionary<int, double>();
			for (int j = 0; j < n; j++) dict[j] = x[j];

			return new SolveResult
			{
				Status = status,
				Message = message,
				VariableValues = dict,
				ObjectiveValue = z
			};
		}
	}
}

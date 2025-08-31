using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThirdYearLP_IP_Solver.HelperClasses;

namespace ThirdYearLP_IP_Solver.Algorithms
{
    internal class B_BKnapsack
    {

        public class KnapsackItem
        {
            public int Index { get; set; }
            public int Value { get; set; }
            public int Weight { get; set; }
            public double Ratio { get; set; }
            public string Name { get; set; }

            public KnapsackItem(int index, int value, int weight)
            {
                Index = index;
                Value = value;
                Weight = weight;
                Ratio = (double)value / weight;
                Name = $"x{index + 1}";
            }
        }

        public class KnapsackNode
        {
            public int Level { get; set; }
            public double Profit { get; set; }
            public int Weight { get; set; }
            public double Bound { get; set; }
            public Dictionary<int, int> FixedVariables { get; set; }
            public KnapsackNode Parent { get; set; }
            public List<KnapsackNode> Children { get; set; }

            public KnapsackNode(int level, double profit, int weight, double bound,
                               Dictionary<int, int> fixedVariables = null, KnapsackNode parent = null)
            {
                Level = level;
                Profit = profit;
                Weight = weight;
                Bound = bound;
                FixedVariables = fixedVariables ?? new Dictionary<int, int>();
                Parent = parent;
                Children = new List<KnapsackNode>();
            }
        }

        public class BranchAndBoundKnapsack
        {
            private List<int> values;
            private List<int> weights;
            private int capacity;
            private List<KnapsackItem> items;
            private double bestValue;
            private List<int> bestSolution;
            private List<int> branchingOrder;

            public BranchAndBoundKnapsack(List<int> values, List<int> weights, int capacity)
            {
                this.values = values;
                this.weights = weights;
                this.capacity = capacity;
                this.items = new List<KnapsackItem>();

                for (int i = 0; i < values.Count; i++)
                {
                    this.items.Add(new KnapsackItem(i, values[i], weights[i]));
                }

                this.bestValue = 0;
                this.bestSolution = new List<int>(new int[values.Count]);

                // Define branching order based on ratio ranking
                var sortedByRatio = Enumerable.Range(0, values.Count)
                    .OrderByDescending(i => (double)values[i] / weights[i])
                    .ToList();
                this.branchingOrder = sortedByRatio;
            }

            public List<KnapsackItem> DisplayRatioTest()
            {
                OutputWriter.WriteLine("Branch & Bound Algorithm - Knapsack Method");
                OutputWriter.WriteLine("Ratio Test");
                OutputWriter.WriteLine("Item   z_i/c_i   Rank");

                // Sort items by ratio in descending order for ranking
                var sortedItems = items.OrderByDescending(x => x.Ratio).ToList();
                var rankMap = new Dictionary<int, int>();

                for (int rank = 0; rank < sortedItems.Count; rank++)
                {
                    rankMap[sortedItems[rank].Index] = rank + 1;
                }

                foreach (var item in items)
                {
                    OutputWriter.WriteLine($"{item.Name}    {item.Value}/{item.Weight} = {item.Ratio:F3}    {rankMap[item.Index]}");
                }
                OutputWriter.WriteLine();

                return sortedItems;
            }

            public double CalculateUpperBound(KnapsackNode node, List<KnapsackItem> sortedItems)
            {
                int remainingCapacity = capacity - node.Weight;
                double upperBound = node.Profit;

                foreach (var item in sortedItems)
                {
                    if (node.FixedVariables.ContainsKey(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        remainingCapacity -= item.Weight;
                        upperBound += item.Value;
                    }
                    else
                    {
                        // Fractional part
                        if (remainingCapacity > 0)
                        {
                            upperBound += ((double)remainingCapacity / item.Weight) * item.Value;
                        }
                        break;
                    }
                }

                return upperBound;
            }

            public double DisplaySubProblem(KnapsackNode node, List<KnapsackItem> sortedItems,
                                           string subProblemNumber, bool isRoot = false)
            {
                if (!string.IsNullOrEmpty(subProblemNumber))
                {
                    OutputWriter.WriteLine($"Sub-Problem {subProblemNumber}");
                }
                else
                {
                    OutputWriter.WriteLine("Sub-Problem");
                }

                int remainingCapacity = capacity;
                double totalValue = 0;

                // Display fixed variables first in order
                var fixedItems = new HashSet<int>();
                var sortedFixedKeys = node.FixedVariables.Keys.OrderBy(k => k).ToList();

                foreach (int itemIndex in sortedFixedKeys)
                {
                    int value = node.FixedVariables[itemIndex];
                    var item = items[itemIndex];

                    if (value == 1)
                    {
                        OutputWriter.WriteLine($"* {item.Name} = {value}    {remainingCapacity}-{item.Weight}={remainingCapacity - item.Weight}");
                        remainingCapacity -= item.Weight;
                        totalValue += item.Value;
                        fixedItems.Add(itemIndex);
                    }
                    else if (value == 0)
                    {
                        OutputWriter.WriteLine($"* {item.Name} = {value}    {remainingCapacity}-0={remainingCapacity}");
                        fixedItems.Add(itemIndex);
                    }
                }

                // Display remaining items based on greedy selection (sorted by ratio)
                foreach (var item in sortedItems)
                {
                    if (fixedItems.Contains(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        OutputWriter.WriteLine($"{item.Name} = 1    {remainingCapacity}-{item.Weight}={remainingCapacity - item.Weight}");
                        remainingCapacity -= item.Weight;
                        totalValue += item.Value;
                    }
                    else if (remainingCapacity > 0)
                    {
                        string fractionalDisplay = $"{remainingCapacity}/{item.Weight}";
                        OutputWriter.WriteLine($"{item.Name} = {fractionalDisplay}    {remainingCapacity}-{item.Weight}");
                        totalValue += ((double)remainingCapacity / item.Weight) * item.Value;
                        remainingCapacity = 0;
                    }
                    else
                    {
                        OutputWriter.WriteLine($"{item.Name} = 0");
                    }
                }

                OutputWriter.WriteLine();
                return totalValue;
            }

            public void DisplayIntegerModel()
            {
                OutputWriter.WriteLine("Integer Programming Model");
                var valueStr = string.Join(" + ", values.Select((v, i) => $"{v}x{i + 1}"));
                OutputWriter.WriteLine($"max z = {valueStr}");

                var weightStr = string.Join(" + ", weights.Select((w, i) => $"{w}x{i + 1}"));
                OutputWriter.WriteLine($"s.t {weightStr} ≤ {capacity}");
                OutputWriter.WriteLine("xi = 0 or 1");
                OutputWriter.WriteLine();
            }

            public bool IsFeasible(KnapsackNode node)
            {
                return node.Weight <= capacity;
            }

            public bool IsComplete(KnapsackNode node)
            {
                return node.FixedVariables.Count == items.Count;
            }

            public int? GetNextVariableToBranch(KnapsackNode node, List<KnapsackItem> sortedItems)
            {
                // First, find fractional variables in the current LP relaxation solution
                int remainingCapacity = capacity - node.Weight;

                foreach (var item in sortedItems)
                {
                    if (node.FixedVariables.ContainsKey(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        remainingCapacity -= item.Weight;
                    }
                    else if (remainingCapacity > 0)
                    {
                        // This variable is fractional - branch on it
                        return item.Index;
                    }
                    else
                    {
                        break;
                    }
                }

                // If no fractional variables, pick the next unfixed variable in ratio order
                foreach (int varIndex in branchingOrder)
                {
                    if (!node.FixedVariables.ContainsKey(varIndex))
                        return varIndex;
                }
                return null;
            }

            private bool IsIntegerRelaxation(KnapsackNode node, List<KnapsackItem> sortedItems)
            {
                int remainingCapacity = capacity - node.Weight;

                foreach (var item in sortedItems)
                {
                    if (node.FixedVariables.ContainsKey(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        remainingCapacity -= item.Weight;
                    }
                    else if (remainingCapacity > 0)
                    {
                        return false; // fractional
                    }
                }

                // Fill greedy completion
                var newFixedVars = new Dictionary<int, int>(node.FixedVariables);
                remainingCapacity = capacity - node.Weight;
                double newProfit = node.Profit;

                foreach (var item in sortedItems)
                {
                    if (newFixedVars.ContainsKey(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        newFixedVars[item.Index] = 1;
                        remainingCapacity -= item.Weight;
                        newProfit += item.Value;
                    }
                    else
                    {
                        newFixedVars[item.Index] = 0;
                    }
                }

                node.FixedVariables = newFixedVars;
                node.Profit = newProfit;
                return true;
            }

            public void SolveRecursive(KnapsackNode node, List<KnapsackItem> sortedItems,
                                      string nodeLabel = "", List<int> candidateCounter = null)
            {
                if (candidateCounter == null)
                    candidateCounter = new List<int> { 0 };

                if (!IsFeasible(node))
                {
                    OutputWriter.WriteLine("Infeasible\n");
                    return;
                }

                // If node is explicitly complete
                if (IsComplete(node))
                {
                    FinalizeCandidate(node, candidateCounter);
                    return;
                }

                // Check if LP relaxation is already integer (implicit completeness)
                int remainingCapacity = capacity - node.Weight;
                bool fractionalFound = false;

                foreach (var item in sortedItems)
                {
                    if (node.FixedVariables.ContainsKey(item.Index))
                        continue;

                    if (remainingCapacity >= item.Weight)
                    {
                        remainingCapacity -= item.Weight;
                    }
                    else if (remainingCapacity > 0)
                    {
                        fractionalFound = true;
                        break;
                    }
                }

                if (!fractionalFound)
                {
                    // Fill unfixed items with 0/1 according to greedy fill
                    var newFixedVars = new Dictionary<int, int>(node.FixedVariables);
                    remainingCapacity = capacity - node.Weight;
                    double newProfit = node.Profit;

                    foreach (var item in sortedItems)
                    {
                        if (newFixedVars.ContainsKey(item.Index))
                            continue;

                        if (remainingCapacity >= item.Weight)
                        {
                            newFixedVars[item.Index] = 1;
                            remainingCapacity -= item.Weight;
                            newProfit += item.Value;
                        }
                        else
                        {
                            newFixedVars[item.Index] = 0;
                        }
                    }

                    node.FixedVariables = newFixedVars;
                    node.Profit = newProfit;

                    // Finalize as candidate BEFORE bound pruning
                    FinalizeCandidate(node, candidateCounter);
                    return;
                }

                // Calculate bound AFTER integer check
                node.Bound = CalculateUpperBound(node, sortedItems);

                // Get next variable to branch on
                int? nextVarIndex = GetNextVariableToBranch(node, sortedItems);
                if (nextVarIndex == null)
                    return;

                // Determine branch labels
                List<string> childLabels;
                string branchDisplay;

                if (string.IsNullOrEmpty(nodeLabel))
                {
                    childLabels = new List<string> { "1", "2" };
                    branchDisplay = $"Sub-P 1: x{nextVarIndex.Value + 1} = 0    Sub-P 2: x{nextVarIndex.Value + 1} = 1";
                }
                else
                {
                    childLabels = new List<string> { $"{nodeLabel}.1", $"{nodeLabel}.2" };
                    branchDisplay = $"Sub-P {nodeLabel}.1: x{nextVarIndex.Value + 1} = 0    Sub-P {nodeLabel}.2: x{nextVarIndex.Value + 1} = 1";
                }

                OutputWriter.WriteLine(branchDisplay);
                OutputWriter.WriteLine("==========================================================");

                // Process both branches
                var branchValues = new int[] { 0, 1 };

                for (int i = 0; i < branchValues.Length; i++)
                {
                    int branchValue = branchValues[i];
                    var newFixedVars = new Dictionary<int, int>(node.FixedVariables);
                    newFixedVars[nextVarIndex.Value] = branchValue;

                    int newWeight = node.Weight;
                    double newProfit = node.Profit;

                    if (branchValue == 1)
                    {
                        newWeight += items[nextVarIndex.Value].Weight;
                        newProfit += items[nextVarIndex.Value].Value;
                    }

                    var newNode = new KnapsackNode(node.Level + 1, newProfit, newWeight, 0, newFixedVars, node);
                    string currentLabel = childLabels[i];
                    DisplaySubProblem(newNode, sortedItems, $"Sub-P {currentLabel}");

                    if (!IsFeasible(newNode))
                    {
                        OutputWriter.WriteLine("Infeasible\n");
                    }
                    else
                    {
                        if (IsIntegerRelaxation(newNode, sortedItems))
                        {
                            FinalizeCandidate(newNode, candidateCounter);
                            continue; // don't branch further
                        }

                        newNode.Bound = CalculateUpperBound(newNode, sortedItems);
                        SolveRecursive(newNode, sortedItems, currentLabel, candidateCounter);
                    }
                }
            }

            private void FinalizeCandidate(KnapsackNode node, List<int> candidateCounter)
            {
                var selectedItems = new List<int>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (node.FixedVariables.ContainsKey(i) && node.FixedVariables[i] == 1)
                        selectedItems.Add(i);
                }

                if (selectedItems.Count > 0)
                {
                    var valueTerms = selectedItems.Select(i => values[i].ToString()).ToArray();
                    OutputWriter.WriteLine($"z = {string.Join(" + ", valueTerms)} = {(int)node.Profit}");
                }
                else
                {
                    OutputWriter.WriteLine("z = 0");
                }

                candidateCounter[0]++;
                char candidateLetter = (char)(65 + candidateCounter[0] - 1);
                OutputWriter.WriteLine($"Candidate {candidateLetter}");

                if (node.Profit > bestValue)
                {
                    bestValue = node.Profit;
                    bestSolution = new List<int>();
                    for (int i = 0; i < items.Count; i++)
                    {
                        bestSolution.Add(node.FixedVariables.ContainsKey(i) ? node.FixedVariables[i] : 0);
                    }
                    OutputWriter.WriteLine("Best Candidate");
                }
                OutputWriter.WriteLine();
            }

            public (double, List<int>) Solve()
            {
                OutputWriter.WriteLine(new string('=', 60));
                var sortedItems = DisplayRatioTest();
                DisplayIntegerModel();

                // Initialize root node
                var rootNode = new KnapsackNode(0, 0, 0, 0);
                rootNode.Bound = CalculateUpperBound(rootNode, sortedItems);

                // Display root problem
                DisplaySubProblem(rootNode, sortedItems, "", true);

                // Start recursive solving
                var candidateCounter = new List<int> { 0 };
                SolveRecursive(rootNode, sortedItems, "", candidateCounter);

                return (bestValue, bestSolution);
            }

            public class KnapSack
            {
                public KnapSack(bool isConsoleOutput = false)
                {

                }

                public void RunBranchAndBoundKnapSack(List<double> objFuncPassed, List<List<double>> constraintsPassed, bool isMin, List<VariableSignType> varSigns = null)
                {
                    // var values = new List<int> { 2, 3, 3, 5, 2, 4 };
                    // var weights = new List<int> { 11, 8, 6, 14, 10, 10 };
                    // int capacity = 40;

                    List<List<double>> constraints = constraintsPassed.Select(x => x.ToList()).ToList();

                    OutputWriter.WriteLine(new string('=', 60));
                    CanonicalFormBuilder.BuildCanonicalForm(objFuncPassed, constraintsPassed, isMin);
                    OutputWriter.WriteLine(new string('=', 60));

                    var values = objFuncPassed.ToList(); ;
                    var weights = constraints[0].GetRange(0, constraints[0].Count - 2);
                    var capacity = (int)constraints[0].ElementAt(constraints[0].Count - 2);

                    // var knapsackSolver = new BranchAndBoundKnapsack(values, weights, capacity);
                    var knapsackSolver = new BranchAndBoundKnapsack(values.Select(v => (int)v).ToList(), weights.Select(v => (int)v).ToList(), capacity);
                    var (bestValue, bestSolution) = knapsackSolver.Solve();

                    OutputWriter.WriteLine(new string('=', 60));
                    OutputWriter.WriteLine("FINAL SOLUTION:");
                    OutputWriter.WriteLine($"Maximum value: {bestValue}");
                    OutputWriter.WriteLine("Solution vector:");

                    for (int i = 0; i < bestSolution.Count; i++)
                    {
                        OutputWriter.WriteLine($"x{i + 1} = {bestSolution[i]}");
                    }

                    // Verify solution
                    int totalWeight = 0;
                    double totalValue = 0;

                    for (int i = 0; i < weights.Count; i++)
                    {
                        totalWeight += (int)(weights[i] * bestSolution[i]);
                        totalValue += values[i] * bestSolution[i];
                    }

                    OutputWriter.WriteLine($"\nVerification:");
                    OutputWriter.WriteLine($"Total weight: {totalWeight} (≤ {capacity})");
                    OutputWriter.WriteLine($"Total value: {totalValue}");
                }
            }
        }
    }



}




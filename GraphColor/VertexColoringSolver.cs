using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace GraphColoring
{
    public class VertexColoringSolver : ISudokuSolver
    {
        private SudokuConnections sudokuGraph;
        private int[][] mappedGrid;
        private Dictionary<int, int?> vertexColors;
        private const int MaxAttempts = 1000000;

        public VertexColoringSolver()
        {
            sudokuGraph = new SudokuConnections();
            mappedGrid = GetMappedMatrix();
            vertexColors = new Dictionary<int, int?>();
        }

        public SudokuGrid Solve(SudokuGrid s)
        {
            InitializeColors(s);
            if (SolveWithBacktracking())
            {
                TransferColorsToGrid(s);
            }
            return s;
        }

        private bool SolveWithBacktracking()
        {
            var uncoloredVertex = FindMostConstrainedVertex();
            if (uncoloredVertex == -1)
                return true;

            var availableColors = GetSortedAvailableColors(uncoloredVertex);
            foreach (var color in availableColors)
            {
                if (IsSafeToColor(uncoloredVertex, color))
                {
                    vertexColors[uncoloredVertex] = color;
                    if (SolveWithBacktracking())
                        return true;
                    vertexColors[uncoloredVertex] = null;
                }
            }
            return false;
        }

        private int FindMostConstrainedVertex()
        {
            int maxConstraints = -1;
            int selectedVertex = -1;

            for (int vertex = 1; vertex <= 81; vertex++)
            {
                if (!vertexColors[vertex].HasValue)
                {
                    int constraints = CountConstraints(vertex);
                    if (constraints > maxConstraints)
                    {
                        maxConstraints = constraints;
                        selectedVertex = vertex;
                    }
                }
            }
            return selectedVertex;
        }

        private int CountConstraints(int vertex)
        {
            int count = 0;
            var usedColors = new HashSet<int>();

            for (int adj = 1; adj <= 81; adj++)
            {
                if (sudokuGraph.Graph.IsNeighbour(vertex, adj) && vertexColors[adj].HasValue)
                {
                    count++;
                    usedColors.Add(vertexColors[adj].Value);
                }
            }
            return count * 10 + usedColors.Count;
        }

        private List<int> GetSortedAvailableColors(int vertex)
        {
            var colorFrequency = new Dictionary<int, int>();
            for (int color = 1; color <= 9; color++)
            {
                colorFrequency[color] = 0;
            }

            for (int adj = 1; adj <= 81; adj++)
            {
                if (sudokuGraph.Graph.IsNeighbour(vertex, adj) && vertexColors[adj].HasValue)
                {
                    var adjColor = vertexColors[adj].Value;
                    colorFrequency[adjColor] = colorFrequency[adjColor] + 1;
                }
            }

            return Enumerable.Range(1, 9)
                .OrderBy(c => colorFrequency[c])
                .ToList();
        }

        private bool IsSafeToColor(int vertex, int color)
        {
            for (int adj = 1; adj <= 81; adj++)
            {
                if (sudokuGraph.Graph.IsNeighbour(vertex, adj) && 
                    vertexColors[adj].HasValue && 
                    vertexColors[adj].Value == color)
                {
                    return false;
                }
            }
            return true;
        }

        private void InitializeColors(SudokuGrid s)
        {
            vertexColors.Clear();
            
            // Initialize all vertices as uncolored
            for (int i = 1; i <= 81; i++)
            {
                vertexColors[i] = null;
            }

            // Set initial colors from the Sudoku grid
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] != 0)
                    {
                        int vertexId = mappedGrid[row][col];
                        vertexColors[vertexId] = s.Cells[row, col];
                    }
                }
            }
        }

        private void ColorGraph()
        {
            bool[] available = new bool[10];
            int attempts = 0;
            bool success = false;

            while (!success && attempts < 100)
            {
                success = TryColorGraph(available);
                attempts++;
            }
        }

        private bool TryColorGraph(bool[] available)
        {
            // Reset all non-fixed colors
            for (int vertex = 1; vertex <= 81; vertex++)
            {
                if (!IsFixedVertex(vertex))
                {
                    vertexColors[vertex] = null;
                }
            }
        
            // Color each uncolored vertex
            for (int vertex = 1; vertex <= 81; vertex++)
            {
                if (!vertexColors[vertex].HasValue)
                {
                    // Reset available colors
                    for (int i = 0; i < 10; i++)
                        available[i] = true;
        
                    // Mark colors of adjacent vertices as unavailable
                    for (int adj = 1; adj <= 81; adj++)
                    {
                        if (sudokuGraph.Graph.IsNeighbour(vertex, adj) && vertexColors[adj].HasValue)
                        {
                            available[vertexColors[adj].Value] = false;
                        }
                    }
        
                    // Get all available colors and shuffle them
                    var possibleColors = new List<int>();
                    for (int color = 1; color <= 9; color++)
                    {
                        if (available[color])
                            possibleColors.Add(color);
                    }
        
                    if (possibleColors.Count == 0)
                        return false;
        
                    // Try a random available color
                    var rnd = new Random();
                    vertexColors[vertex] = possibleColors[rnd.Next(possibleColors.Count)];
                }
            }
        
            return IsValidColoring();
        }

        private bool IsFixedVertex(int vertex)
        {
            return vertexColors.ContainsKey(vertex) && 
                   vertexColors[vertex].HasValue && 
                   IsPartOfInitialGrid(vertex);
        }

        private bool IsPartOfInitialGrid(int vertex)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (mappedGrid[row][col] == vertex)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void TransferColorsToGrid(SudokuGrid s)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int vertexId = mappedGrid[row][col];
                    if (vertexColors[vertexId].HasValue)
                    {
                        s.Cells[row, col] = vertexColors[vertexId].Value;
                    }
                }
            }
        }

        private bool IsValidColoring()
        {
            // Check if all vertices are colored
            if (vertexColors.Values.Any(c => !c.HasValue))
                return false;

            // Check if adjacent vertices have different colors
            for (int v = 1; v <= 81; v++)
            {
                for (int u = v + 1; u <= 81; u++)
                {
                    if (sudokuGraph.Graph.IsNeighbour(v, u) && 
                        vertexColors[v].Value == vertexColors[u].Value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int[][] GetMappedMatrix()
        {
            var matrix = new int[9][];
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int count = 1;
            for (int rows = 0; rows < 9; rows++)
            {
                for (int cols = 0; cols < 9; cols++)
                {
                    matrix[rows][cols] = count++;
                }
            }
            return matrix;
        }
    }
}

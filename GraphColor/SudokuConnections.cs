using System.Collections.Generic;

namespace GraphColoring
{
    public class SudokuConnections
    {
        public Graph Graph { get; private set; }
        private readonly int Rows = 9;
        private readonly int Cols = 9;
        private readonly int TotalBlocks;
        public IEnumerable<int> AllIds { get; private set; }

        public SudokuConnections()
        {
            Graph = new Graph();
            TotalBlocks = Rows * Cols;
            GenerateGraph();
            ConnectEdges();
            AllIds = Graph.GetAllNodesIds();
        }

        private void GenerateGraph()
        {
            for (int idx = 1; idx <= TotalBlocks; idx++)
            {
                Graph.AddNode(idx);
            }
        }

        private void ConnectEdges()
        {
            var matrix = GetGridMatrix();
            var headConnections = new Dictionary<int, Dictionary<string, List<int>>>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int head = matrix[row][col];
                    var connections = WhatToConnect(matrix, row, col);
                    headConnections[head] = connections;
                }
            }

            ConnectThose(headConnections);
        }

        private void ConnectThose(Dictionary<int, Dictionary<string, List<int>>> headConnections)
        {
            foreach (var head in headConnections.Keys)
            {
                var connections = headConnections[head];
                foreach (var key in connections.Keys)
                {
                    foreach (var v in connections[key])
                    {
                        Graph.AddEdge(head, v);
                    }
                }
            }
        }

        private Dictionary<string, List<int>> WhatToConnect(int[][] matrix, int rows, int cols)
        {
            var connections = new Dictionary<string, List<int>>
            {
                ["rows"] = new List<int>(),
                ["cols"] = new List<int>(),
                ["blocks"] = new List<int>()
            };

            // ROWS
            for (int c = cols + 1; c < 9; c++)
            {
                connections["rows"].Add(matrix[rows][c]);
            }

            // COLS
            for (int r = rows + 1; r < 9; r++)
            {
                connections["cols"].Add(matrix[r][cols]);
            }

            // BLOCKS
            if (rows % 3 == 0)
            {
                if (cols % 3 == 0)
                {
                    connections["blocks"].Add(matrix[rows + 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols + 2]);
                    connections["blocks"].Add(matrix[rows + 2][cols + 1]);
                    connections["blocks"].Add(matrix[rows + 2][cols + 2]);
                }
                else if (cols % 3 == 1)
                {
                    connections["blocks"].Add(matrix[rows + 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows + 2][cols - 1]);
                    connections["blocks"].Add(matrix[rows + 2][cols + 1]);
                }
                else if (cols % 3 == 2)
                {
                    connections["blocks"].Add(matrix[rows + 1][cols - 2]);
                    connections["blocks"].Add(matrix[rows + 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows + 2][cols - 2]);
                    connections["blocks"].Add(matrix[rows + 2][cols - 1]);
                }
            }
            else if (rows % 3 == 1)
            {
                if (cols % 3 == 0)
                {
                    connections["blocks"].Add(matrix[rows - 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols + 2]);
                    connections["blocks"].Add(matrix[rows + 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols + 2]);
                }
                else if (cols % 3 == 1)
                {
                    connections["blocks"].Add(matrix[rows - 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols + 1]);
                }
                else if (cols % 3 == 2)
                {
                    connections["blocks"].Add(matrix[rows - 1][cols - 2]);
                    connections["blocks"].Add(matrix[rows - 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows + 1][cols - 2]);
                    connections["blocks"].Add(matrix[rows + 1][cols - 1]);
                }
            }
            else if (rows % 3 == 2)
            {
                if (cols % 3 == 0)
                {
                    connections["blocks"].Add(matrix[rows - 2][cols + 1]);
                    connections["blocks"].Add(matrix[rows - 2][cols + 2]);
                    connections["blocks"].Add(matrix[rows - 1][cols + 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols + 2]);
                }
                else if (cols % 3 == 1)
                {
                    connections["blocks"].Add(matrix[rows - 2][cols - 1]);
                    connections["blocks"].Add(matrix[rows - 2][cols + 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols - 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols + 1]);
                }
                else if (cols % 3 == 2)
                {
                    connections["blocks"].Add(matrix[rows - 2][cols - 2]);
                    connections["blocks"].Add(matrix[rows - 2][cols - 1]);
                    connections["blocks"].Add(matrix[rows - 1][cols - 2]);
                    connections["blocks"].Add(matrix[rows - 1][cols - 1]);
                }
            }

            return connections;
        }

        private int[][] GetGridMatrix()
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
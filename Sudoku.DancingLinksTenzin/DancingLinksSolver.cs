using System;
using System.Collections.Generic;
using System.Linq;
using DlxLib;
using Sudoku.Shared;

namespace Sudoku.DancingLinksTenzin
{
    public class DancingLinksSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            var (matrix, constraintsMap) = ConvertToExactCoverMatrix(s);
            var solver = new Dlx();
            var solution = solver.Solve(matrix, row => row, row => row).FirstOrDefault();
            
            if (solution != null)
            {
                var solvedGrid = ConvertToSudokuGrid(solution.RowIndexes, constraintsMap, s);
                Console.WriteLine("Solution trouvée par DLX:");
                Console.WriteLine(solvedGrid.ToString());
                return IsValidSolution(solvedGrid) ? solvedGrid : s.CloneSudoku();
            }
            
            Console.WriteLine("Aucune solution trouvée");
            return s.CloneSudoku();
        }

        private (List<int[]>, Dictionary<int, (int, int, int)>) ConvertToExactCoverMatrix(SudokuGrid grid)
        {
            int size = 9;
            var matrix = new List<int[]>();
            var constraintsMap = new Dictionary<int, (int, int, int)>();
            int rowIndex = 0;

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    int value = grid.Cells[r, c];
                    if (value == 0)
                    {
                        for (int num = 1; num <= size; num++)
                        {
                            matrix.Add(CreateRow(r, c, num));
                            constraintsMap[rowIndex++] = (r, c, num);
                        }
                    }
                    else
                    {
                        matrix.Add(CreateRow(r, c, value));
                        constraintsMap[rowIndex++] = (r, c, value);
                    }
                }
            }
            return (matrix, constraintsMap);
        }

        private int[] CreateRow(int r, int c, int num)
        {
            int size = 9;
            int boxSize = 3;
            int numCols = size * size * 4;
            int[] row = new int[numCols];

            int cellConstraint = r * size + c;
            int rowConstraint = size * size + r * size + (num - 1);
            int colConstraint = size * size * 2 + c * size + (num - 1);
            int boxConstraint = size * size * 3 + ((r / boxSize) * boxSize + (c / boxSize)) * size + (num - 1);
            
            row[cellConstraint] = 1;
            row[rowConstraint] = 1;
            row[colConstraint] = 1;
            row[boxConstraint] = 1;

            return row;
        }

        private SudokuGrid ConvertToSudokuGrid(IEnumerable<int> solution, Dictionary<int, (int, int, int)> constraintsMap, SudokuGrid original)
        {
            var solvedGrid = original.CloneSudoku();
            foreach (var index in solution)
            {
                if (constraintsMap.ContainsKey(index))
                {
                    var (row, col, num) = constraintsMap[index];
                    solvedGrid.Cells[row, col] = num;
                }
            }
            return solvedGrid;
        }

        private bool IsValidSolution(SudokuGrid grid)
        {
            return Enumerable.Range(0, 9).All(i => IsValidSet(GetRow(grid, i)) && IsValidSet(GetColumn(grid, i)) && IsValidSet(GetBox(grid, i)));
        }

        private int[] GetRow(SudokuGrid grid, int row)
        {
            return Enumerable.Range(0, 9).Select(col => grid.Cells[row, col]).ToArray();
        }

        private int[] GetColumn(SudokuGrid grid, int col)
        {
            return Enumerable.Range(0, 9).Select(row => grid.Cells[row, col]).ToArray();
        }

        private int[] GetBox(SudokuGrid grid, int boxIndex)
        {
            int boxRow = (boxIndex / 3) * 3;
            int boxCol = (boxIndex % 3) * 3;
            var values = new List<int>();
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    values.Add(grid.Cells[boxRow + r, boxCol + c]);
                }
            }
            return values.ToArray();
        }

        private bool IsValidSet(int[] numbers)
        {
            var filteredNumbers = numbers.Where(n => n > 0).ToArray();
            return filteredNumbers.Distinct().Count() == filteredNumbers.Length;
        }
    }
}

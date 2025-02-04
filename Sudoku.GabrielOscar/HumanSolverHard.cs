using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Solvers
{
    public class HumanSolverHard : ISudokuSolver
    {
        private const int MAX_ITERATIONS = 200;

        public SudokuGrid Solve(SudokuGrid grid)
        {
            Console.WriteLine("Début de la résolution du Sudoku difficile...");
            bool progress;
            int iteration = 0;

            do
            {
                progress = false;
                progress |= ApplyXYZWing(grid);
                progress |= ApplyFinnedXWing(grid);
                progress |= ApplyForcingChains(grid);
                iteration++;

                if (iteration >= MAX_ITERATIONS)
                {
                    Console.WriteLine("Aucune progression après plusieurs tentatives, passage au backtracking avancé...");
                    break;
                }
            } while (progress && !IsSolved(grid));

            if (!IsSolved(grid))
            {
                Console.WriteLine("Passage au backtracking optimisé...");
                BacktrackingSolve(grid);
            }

            Console.WriteLine("Solveur difficile terminé.");
            return grid;
        }

        private bool ApplyXYZWing(SudokuGrid grid)
        {
            bool progress = false;
            var candidateMap = new Dictionary<(int row, int col), List<int>>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var candidates = GetCandidates(grid, row, col);
                        if (candidates.Count == 3)
                        {
                            candidateMap[(row, col)] = candidates;
                        }
                    }
                }
            }

            foreach (var ((row1, col1), candidates1) in candidateMap)
            {
                foreach (var ((row2, col2), candidates2) in candidateMap)
                {
                    if ((row1, col1) != (row2, col2) && candidates1.Intersect(candidates2).Count() == 2)
                    {
                        foreach (var ((row3, col3), candidates3) in candidateMap)
                        {
                            if ((row3, col3) != (row1, col1) && (row3, col3) != (row2, col2) &&
                                candidates3.Intersect(candidates1).Count() == 2 &&
                                candidates3.Intersect(candidates2).Count() == 2)
                            {
                                foreach (var (row, col) in GetRow(grid, row3).Concat(GetColumn(grid, col3)))
                                {
                                    if (grid.Cells[row, col] == 0)
                                    {
                                        var cellCandidates = GetCandidates(grid, row, col);
                                        if (cellCandidates.Contains(candidates1.Intersect(candidates2).First()))
                                        {
                                            cellCandidates.Remove(candidates1.Intersect(candidates2).First());
                                            progress = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private bool ApplyFinnedXWing(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation de Finned X-Wing
            for (int digit = 1; digit <= 9; digit++)
            {
                for (int row1 = 0; row1 < 8; row1++)
                {
                    for (int row2 = row1 + 1; row2 < 9; row2++)
                    {
                        var colsWithCandidates = new List<int>();
                        for (int col = 0; col < 9; col++)
                        {
                            if (grid.Cells[row1, col] == 0 && grid.Cells[row2, col] == 0)
                            {
                                var c1 = GetCandidates(grid, row1, col);
                                var c2 = GetCandidates(grid, row2, col);
                                if (c1.Contains(digit) && c2.Contains(digit))
                                    colsWithCandidates.Add(col);
                            }
                        }
                        if (colsWithCandidates.Count == 2)
                        {
                            foreach (var col in colsWithCandidates)
                            {
                                for (int row = 0; row < 9; row++)
                                {
                                    if (row != row1 && row != row2 && grid.Cells[row, col] == 0)
                                    {
                                        var cellCandidates = GetCandidates(grid, row, col);
                                        if (cellCandidates.Contains(digit))
                                        {
                                            cellCandidates.Remove(digit);
                                            progress = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private bool ApplyForcingChains(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation de Forcing Chains
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var candidates = GetCandidates(grid, row, col);
                        foreach (var candidate in candidates)
                        {
                            if (CheckForcingChain(grid, row, col, candidate))
                            {
                                grid.Cells[row, col] = candidate;
                                progress = true;
                                break;
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private bool CheckForcingChain(SudokuGrid grid, int row, int col, int candidate)
        {
            var testGrid = grid.CloneSudoku();
            testGrid.Cells[row, col] = candidate;

            var deductions = new HashSet<int>();
            for (int i = 0; i < 9; i++)
            {
                deductions.Add(testGrid.Cells[row, i]);
                deductions.Add(testGrid.Cells[i, col]);
            }
            return deductions.Count == 9;
        }
        private bool BacktrackingSolve(SudokuGrid grid)
        {
            var emptyCell = GetBestCellForExploration(grid);
            if (emptyCell == null)
                return true;

            var (row, col) = emptyCell.Value;
            var candidates = GetCandidates(grid, row, col).OrderBy(c => GetConstraintScore(grid, row, col, c)).ToList();
            foreach (var candidate in candidates)
            {
                grid.Cells[row, col] = candidate;
                if (BacktrackingSolve(grid))
                    return true;
                grid.Cells[row, col] = 0;
            }
            return false;
        }

        private int GetConstraintScore(SudokuGrid grid, int row, int col, int value)
        {
            int score = 0;
            foreach (var (r, c) in GetRow(grid, row).Concat(GetColumn(grid, col)).Concat(GetBox(grid, (row / 3) * 3 + col / 3)))
            {
                if (grid.Cells[r, c] == 0 && GetCandidates(grid, r, c).Contains(value))
                {
                    score++;
                }
            }
            return score;
        }
        private (int row, int col)? GetBestCellForExploration(SudokuGrid grid)
        {
            (int row, int col)? bestCell = null;
            int minCandidates = int.MaxValue;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var candidates = GetCandidates(grid, row, col);
                        if (candidates.Count < minCandidates)
                        {
                            minCandidates = candidates.Count;
                            bestCell = (row, col);
                        }
                    }
                }
            }
            return bestCell;
        }

        private List<(int row, int col)> GetRow(SudokuGrid grid, int row)
        {
            return Enumerable.Range(0, 9).Select(col => (row, col)).ToList();
        }

        private List<(int row, int col)> GetColumn(SudokuGrid grid, int col)
        {
            return Enumerable.Range(0, 9).Select(row => (row, col)).ToList();
        }

        private List<(int row, int col)> GetBox(SudokuGrid grid, int boxIndex)
        {
            int startRow = (boxIndex / 3) * 3;
            int startCol = (boxIndex % 3) * 3;
            var cells = new List<(int row, int col)>();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    cells.Add((startRow + i, startCol + j));
                }
            }
            return cells;
        }

        private bool IsSolved(SudokuGrid grid)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                        return false;
                }
            }
            return true;
        }
        private List<int> GetCandidates(SudokuGrid grid, int row, int col)
        {
            var used = new HashSet<int>();
            for (int i = 0; i < 9; i++)
            {
                used.Add(grid.Cells[row, i]);
                used.Add(grid.Cells[i, col]);
            }
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    used.Add(grid.Cells[startRow + i, startCol + j]);
                }
            }
            return Enumerable.Range(1, 9).Where(n => !used.Contains(n)).ToList();
        }
    }
}
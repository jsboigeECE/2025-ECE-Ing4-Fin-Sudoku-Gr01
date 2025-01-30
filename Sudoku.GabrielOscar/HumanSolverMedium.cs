using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Solvers
{
    public class HumanSolverMedium : ISudokuSolver
    {
        private const int MAX_ITERATIONS = 1000;
        public SudokuGrid Solve(SudokuGrid grid)
        {
            Console.WriteLine("Début de la résolution du Sudoku...");
            bool progress;
            int iteration = 0;
            do
            {
                progress = false;
                progress |= ApplySingleCandidate(grid);
                progress |= ApplyHiddenSingle(grid);
                progress |= ApplyIntersectionRemoval(grid);
                progress |= ApplyXYWing(grid);
                progress |= ApplyXWing(grid);
                progress |= ApplySwordfish(grid);
                iteration++;
                if (iteration >= MAX_ITERATIONS)
                {
                    Console.WriteLine("Aucune progression après plusieurs tentatives, passage au backtracking...");
                    break;
                }
            } while (progress && !IsSolved(grid));
            if (!IsSolved(grid))
            {
                Console.WriteLine("Passage au backtracking optimisé...");
                BacktrackingSolve(grid);
            }
            Console.WriteLine("Solveur terminé.");
            return grid;
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
        private bool ApplySingleCandidate(SudokuGrid grid)
        {
            bool progress = false;
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var candidates = GetCandidates(grid, row, col);
                        if (candidates.Count == 1)
                        {
                            grid.Cells[row, col] = candidates[0];
                            progress = true;
                        }
                    }
                }
            }
            return progress;
        }

        private bool ApplyHiddenSingle(SudokuGrid grid)
        {
            bool progress = false;
            for (int i = 0; i < 9; i++)
            {
                progress |= ApplyHiddenSingleInGroup(grid, GetRow(grid, i));
                progress |= ApplyHiddenSingleInGroup(grid, GetColumn(grid, i));
                progress |= ApplyHiddenSingleInGroup(grid, GetBox(grid, i));
            }
            return progress;
        }

        private bool ApplyHiddenSingleInGroup(SudokuGrid grid, List<(int row, int col)> group)
        {
            bool progress = false;
            var candidateMap = new Dictionary<int, List<(int row, int col)>>();

            foreach (var (row, col) in group)
            {
                if (grid.Cells[row, col] == 0)
                {
                    var candidates = GetCandidates(grid, row, col);
                    foreach (var candidate in candidates)
                    {
                        if (!candidateMap.ContainsKey(candidate))
                        {
                            candidateMap[candidate] = new List<(int row, int col)>();
                        }
                        candidateMap[candidate].Add((row, col));
                    }
                }
            }

            foreach (var kvp in candidateMap)
            {
                if (kvp.Value.Count == 1)
                {
                    var (row, col) = kvp.Value[0];
                    grid.Cells[row, col] = kvp.Key;
                    progress = true;
                }
            }
            return progress;
        }

        private bool ApplyIntersectionRemoval(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation complète de l'Intersection Removal
            for (int i = 0; i < 9; i++)
            {
                progress |= ApplyIntersectionRemovalInGroup(grid, GetRow(grid, i));
                progress |= ApplyIntersectionRemovalInGroup(grid, GetColumn(grid, i));
            }
            return progress;
        }

        private bool ApplyIntersectionRemovalInGroup(SudokuGrid grid, List<(int row, int col)> group)
        {
            bool progress = false;
            var candidateMap = new Dictionary<int, List<(int row, int col)>>();

            foreach (var (row, col) in group)
            {
                if (grid.Cells[row, col] == 0)
                {
                    var candidates = GetCandidates(grid, row, col);
                    foreach (var candidate in candidates)
                    {
                        if (!candidateMap.ContainsKey(candidate))
                        {
                            candidateMap[candidate] = new List<(int row, int col)>();
                        }
                        candidateMap[candidate].Add((row, col));
                    }
                }
            }

            foreach (var kvp in candidateMap)
            {
                if (kvp.Value.Count > 1 && kvp.Value.Count <= 3)
                {
                    var (firstRow, firstCol) = kvp.Value[0];
                    bool sameRow = kvp.Value.All(c => c.row == firstRow);
                    bool sameCol = kvp.Value.All(c => c.col == firstCol);
                    
                    if (sameRow || sameCol)
                    {
                        foreach (var (row, col) in group)
                        {
                            if (!kvp.Value.Contains((row, col)) && grid.Cells[row, col] == 0)
                            {
                                var cellCandidates = GetCandidates(grid, row, col);
                                if (cellCandidates.Contains(kvp.Key))
                                {
                                    cellCandidates.Remove(kvp.Key);
                                    progress = true;
                                }
                            }
                        }
                    }
                }
            }
            return progress;
        }

        private bool ApplyXYWing(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation complète de XY-Wing
            var candidateCells = new Dictionary<(int row, int col), List<int>>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        var candidates = GetCandidates(grid, row, col);
                        if (candidates.Count == 2)
                        {
                            candidateCells[(row, col)] = candidates;
                        }
                    }
                }
            }

            foreach (var ((row1, col1), candidates1) in candidateCells)
            {
                foreach (var ((row2, col2), candidates2) in candidateCells)
                {
                    if ((row1, col1) != (row2, col2) && candidates1.Intersect(candidates2).Count() == 1)
                    {
                        foreach (var ((row3, col3), candidates3) in candidateCells)
                        {
                            if ((row3, col3) != (row1, col1) && (row3, col3) != (row2, col2) &&
                                candidates3.Intersect(candidates1).Count() == 1 &&
                                candidates3.Intersect(candidates2).Count() == 1)
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

        private bool ApplyXWing(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation complète de X-Wing
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

        private bool ApplySwordfish(SudokuGrid grid)
        {
            bool progress = false;
            // Implémentation complète de Swordfish
            for (int digit = 1; digit <= 9; digit++)
            {
                for (int row1 = 0; row1 < 7; row1++)
                {
                    for (int row2 = row1 + 1; row2 < 8; row2++)
                    {
                        for (int row3 = row2 + 1; row3 < 9; row3++)
                        {
                            var colsWithCandidates = new List<int>();
                            for (int col = 0; col < 9; col++)
                            {
                                if (grid.Cells[row1, col] == 0 && grid.Cells[row2, col] == 0 && grid.Cells[row3, col] == 0)
                                {
                                    var c1 = GetCandidates(grid, row1, col);
                                    var c2 = GetCandidates(grid, row2, col);
                                    var c3 = GetCandidates(grid, row3, col);
                                    if (c1.Contains(digit) && c2.Contains(digit) && c3.Contains(digit))
                                        colsWithCandidates.Add(col);
                                }
                            }
                            if (colsWithCandidates.Count == 3)
                            {
                                foreach (var col in colsWithCandidates)
                                {
                                    for (int row = 0; row < 9; row++)
                                    {
                                        if (row != row1 && row != row2 && row != row3 && grid.Cells[row, col] == 0)
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
            }
            return progress;
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
    }
}
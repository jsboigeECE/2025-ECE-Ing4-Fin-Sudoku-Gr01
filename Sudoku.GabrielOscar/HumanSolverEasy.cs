using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.Solvers
{
    public class HumanSolverEasy : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid grid)
        {
            bool progress;
            do
            {
                progress = false;

                // Appliquer les techniques simples
                progress |= ApplySingleCandidate(grid);
                progress |= ApplyHiddenSingle(grid);

            } while (progress);

            return grid;
        }

        private bool ApplySingleCandidate(SudokuGrid grid)
        {
            bool progress = false;

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0) // Case vide
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

            // Parcourir les lignes, colonnes et boîtes
            for (int i = 0; i < 9; i++)
            {
                // Lignes
                progress |= ApplyHiddenSingleInGroup(grid, GetRow(grid, i));
                // Colonnes
                progress |= ApplyHiddenSingleInGroup(grid, GetColumn(grid, i));
                // Boîtes
                progress |= ApplyHiddenSingleInGroup(grid, GetBox(grid, i));
            }

            return progress;
        }

        private bool ApplyHiddenSingleInGroup(SudokuGrid grid, List<(int row, int col)> group)
        {
            bool progress = false;
            var candidateMap = new Dictionary<int, List<(int row, int col)>>();

            // Construire la carte des candidats
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

            // Vérifier les candidats uniques
            foreach (var kvp in candidateMap)
            {
                if (kvp.Value.Count == 1) // Candidat unique
                {
                    var (row, col) = kvp.Value[0];
                    grid.Cells[row, col] = kvp.Key;
                    progress = true;
                }
            }

            return progress;
        }

        private List<int> GetCandidates(SudokuGrid grid, int row, int col)
        {
            var used = new HashSet<int>();

            // Vérifier la ligne et la colonne
            for (int i = 0; i < 9; i++)
            {
                used.Add(grid.Cells[row, i]);
                used.Add(grid.Cells[i, col]);
            }

            // Vérifier la boîte
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    used.Add(grid.Cells[startRow + i, startCol + j]);
                }
            }

            // Retourner les numéros restants
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

using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.AIMASolvers
{

        public class AIMAsimpleSolver : ISudokuSolver
        {
            public SudokuGrid Solve(SudokuGrid s)
            {
                var solvedGrid = s.CloneSudoku();
                if (BacktrackingSearch(solvedGrid))
                {
                    return solvedGrid;
                }
                return s; // Retourne la grille originale si aucune solution trouvée
            }

            private bool BacktrackingSearch(SudokuGrid grid)
            {
                (int row, int col)? emptyCell = FindUnassignedCell(grid);
                if (emptyCell == null)
                {
                    return true; // Toutes les cellules sont remplies, la solution est trouvée
                }

                (int row, int col) = emptyCell.Value;
                foreach (var num in OrderDomainValues(grid, row, col))
                {
                    if (IsValid(grid, row, col, num))
                    {
                        grid.Cells[row, col] = num;
                        if (BacktrackingSearch(grid))
                        {
                            return true;
                        }
                        grid.Cells[row, col] = 0; // Annule la tentative
                    }
                }
                return false;
            }

            private (int row, int col)? FindUnassignedCell(SudokuGrid grid)
            {
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (grid.Cells[row, col] == 0)
                        {
                            return (row, col);
                        }
                    }
                }
                return null;
            }

            private IEnumerable<int> OrderDomainValues(SudokuGrid grid, int row, int col)
            {
                var possibleValues = grid.GetAvailableNumbers(row, col);
                return possibleValues.OrderBy(val => Guid.NewGuid()); // Mélange aléatoire pour explorer différentes solutions
            }

            private bool IsValid(SudokuGrid grid, int row, int col, int num)
            {
                foreach (var (r, c) in SudokuGrid.CellNeighbours[row][col])
                {
                    if (grid.Cells[r, c] == num)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

    
}

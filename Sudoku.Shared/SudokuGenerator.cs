using System;
using System.Collections.Generic;

namespace Sudoku.Shared
{
    // Classe pour générer un Sudoku valide
    public class SudokuGenerator
    {
        private static Random _random = new Random();

        public SudokuGrid GenerateSudoku()
        {
            var gridSudoku = new SudokuGrid();
            int[,] sudoku = gridSudoku.Cells;

            // Remplir la diagonale 3x3
            for (int i = 0; i < 9; i += 3)
            {
                FillDiagonalBlock(sudoku, i, i);
            }

            // Remplir le reste
            if (FillRemaining(sudoku))
            {
                return gridSudoku;
            }

            return null; // Retourne null si aucun Sudoku valide n'a pu être généré
        }

        private static void FillDiagonalBlock(int[,] sudoku, int rowStart, int colStart)
        {
            int[] nums = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(nums);

            int index = 0;
            for (int i = rowStart; i < rowStart + 3; i++)
            {
                for (int j = colStart; j < colStart + 3; j++)
                {
                    sudoku[i, j] = nums[index++];
                }
            }
        }

        private static bool FillRemaining(int[,] sudoku)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (sudoku[i, j] == 0)
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsSafe(sudoku, i, j, num))
                            {
                                sudoku[i, j] = num;

                                if (FillRemaining(sudoku))
                                {
                                    return true;
                                }

                                sudoku[i, j] = 0;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsSafe(int[,] sudoku, int row, int col, int num)
        {
            for (int i = 0; i < 9; i++)
            {
                if (sudoku[row, i] == num || sudoku[i, col] == num)
                {
                    return false;
                }
            }

            int startRow = row - row % 3;
            int startCol = col - col % 3;

            for (int i = startRow; i < startRow + 3; i++)
            {
                for (int j = startCol; j < startCol + 3; j++)
                {
                    if (sudoku[i, j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void Shuffle(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
    }

    // Classe pour résoudre le Sudoku
    public class SudokuSolver
    {
        public bool Solve(SudokuGrid grid)
        {
            return SolveCSP(grid.Cells);
        }

        private static bool SolveCSP(int[,] sudoku)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (sudoku[i, j] == 0)
                    {
                        for (int num = 1; num <= 9; num++)
                        {
                            if (IsSafe(sudoku, i, j, num))
                            {
                                sudoku[i, j] = num;

                                if (SolveCSP(sudoku))
                                {
                                    return true;
                                }

                                sudoku[i, j] = 0;
                            }
                        }

                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsSafe(int[,] sudoku, int row, int col, int num)
        {
            for (int i = 0; i < 9; i++)
            {
                if (sudoku[row, i] == num || sudoku[i, col] == num)
                {
                    return false;
                }
            }

            int startRow = row - row % 3;
            int startCol = col - col % 3;

            for (int i = startRow; i < startRow + 3; i++)
            {
                for (int j = startCol; j < startCol + 3; j++)
                {
                    if (sudoku[i, j] == num)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

using System;

namespace Sudoku.Shared
{
    public static class SudokuPrinter
    {
        public static void PrintSudoku(int[][] grid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(grid[i][j] == 0 ? "." : grid[i][j].ToString());
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}

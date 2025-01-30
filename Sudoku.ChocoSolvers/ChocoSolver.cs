using Sudoku.Shared;
using System;

namespace Sudoku.ChocoSolvers;

public class ChocoSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
      
        
        // Résoudre avec Choco Solver
         ResoudreSudoku(s.Cells);
         return s;

    }

   
    private bool ResoudreSudoku(int[,] grid)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (grid[row, col] == 0)
                {
                    for (int num = 1; num <= 9; num++)
                    {
                        if (EstValide(grid, row, col, num))
                        {
                            grid[row, col] = num;
                            if (ResoudreSudoku(grid))
                                return true;
                            grid[row, col] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private bool EstValide(int[,] grid, int row, int col, int num)
    {
        for (int i = 0; i < 9; i++)
        {
            if (grid[row, i] == num || grid[i, col] == num)
                return false;
        }
        
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[startRow + i, startCol + j] == num)
                    return false;
            }
        }
        
        return true;
    }
	
}

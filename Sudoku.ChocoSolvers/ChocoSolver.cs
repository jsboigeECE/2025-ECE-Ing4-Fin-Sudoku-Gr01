using Sudoku.Shared;
using System;

namespace Sudoku.ChocoSolvers;

public class ChocoSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        // Convertir la grille en tableau
        int[,] sudokuArray = SudokuGridToArray(s);
        
        // Résoudre avec Choco Solver
        int[,] solutionArray = SolveWithChoco(sudokuArray);
        
        // Convertir la solution en SudokuGrid
        return ArrayToSudokuGrid(solutionArray);
    }

    private int[,] SolveWithChoco(int[,] sudoku)
    {
        int[,] solution = CopierGrille(sudoku);
        
        // Appliquer un algorithme de backtracking simple
        if (ResoudreSudoku(solution))
        {
            return solution;
        }
        else
        {
            throw new Exception("Pas de solution trouvée");
        }
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

    private int[,] CopierGrille(int[,] source)
    {
        int[,] copie = new int[9, 9];
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                copie[r, c] = source[r, c];
            }
        }
        return copie;
    }

    private int[,] SudokuGridToArray(SudokuGrid s)
    {
        int[,] array = new int[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                array[i, j] = s.Cells[i, j];
            }
        }
        return array;
    }

    private SudokuGrid ArrayToSudokuGrid(int[,] array)
    {
        SudokuGrid s = new SudokuGrid();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                s.Cells[i, j] = array[i, j];
            }
        }
        return s;
    }
}

using Sudoku.Shared;
using System;
using org.chocosolver.solver.variables;

namespace Sudoku.ChocoSolvers;

// Je garde votre classe initiale, mais vous serez amenés à encréer plusieurs, typiquement héritant d'une classe de base et changeant les nombreux paramètres disponibles
public class ChocoSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
      
        
        // Résoudre avec Choco Solver
         ResoudreSudokuBacktracking(s.Cells);
         return s;

    }


    private void ResoudreSudokuChocoSolver(SudokuGrid s)
    {
        // Je fais juste l'amorce pour vous montrer où vous pouvez trouver le code de Choco. Le reste sera dans la doc
	    var model = new org.chocosolver.solver.Model();
	    var cellVariables = model.intVarMatrix(9, 9, 1, 9);
		// cf la doc pour la suite:
		// https://choco-solver.org/docs/modeling/intconstraints/
		// cf par exemple https://choco-solver.org/docs/solving/strategies/
		// https://choco-solver.org/docs/advanced-usages/strategies/
		//https://choco-solver.org/docs/solving/lns/
    }



	// Je laisse votre ancien code que j'ai juste simplifié, mais vous pourrez l'enlever
	private bool ResoudreSudokuBacktracking(int[,] grid)
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
                            if (ResoudreSudokuBacktracking(grid))
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

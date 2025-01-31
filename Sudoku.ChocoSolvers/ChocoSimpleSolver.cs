﻿using Sudoku.Shared;
using System;
using org.chocosolver.solver;
using org.chocosolver.solver.constraints;
using org.chocosolver.solver.variables;

namespace Sudoku.ChocoSolvers
{
    public class ChocoSimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            ResoudreSudokuChocoSolver(s);
            return s;
        }

        private void ResoudreSudokuChocoSolver(SudokuGrid s)
        {
            var model = new Model("Sudoku Solver");
            var cellVariables = model.intVarMatrix("cells", 9, 9, 1, 9);

            var constraints = new List<Constraint>();
            Constraint currentConstraint;
            // Contraintes pour les lignes et les colonnes
            for (int i = 0; i < 9; i++)
            {
	            // Lignes
				currentConstraint = model.allDifferent(cellVariables[i]); 
                constraints.Add(currentConstraint);
                // Colonnes
				currentConstraint = model.allDifferent(GetColumn(cellVariables, i));
                constraints.Add(currentConstraint);
            }

            // Contraintes pour les blocs 3x3
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
	                currentConstraint = model.allDifferent(GetBlock(cellVariables, blockRow, blockCol));
					constraints.Add(currentConstraint);
                }
            }

            // Appliquer les valeurs initiales du Sudoku
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] != 0)
                    {
	                    currentConstraint = model.arithm(cellVariables[row][col], "=", s.Cells[row, col]);
						constraints.Add(currentConstraint);
						
                    }
                }
            }

            // Résolution du modèle
           
			 var solver = GetSolver(model, cellVariables, constraints);
            if (solver.solve())
            {
                // Remplissage du SudokuGrid avec la solution trouvée
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        s.Cells[row, col] = cellVariables[row][col].getValue();
                    }
                }
            }
        }

        public virtual Solver GetSolver(Model model, IntVar[][] cellVariables, List<Constraint> constraints)
        {
			var solver = model.getSolver();
			
			foreach (var constraint in constraints)
			{
				constraint.post();
			}

			return solver;
        }

		// Méthodes auxiliaires pour récupérer colonnes et blocs
		private IntVar[] GetColumn(IntVar[][] grid, int col)
        {
            return Enumerable.Range(0, 9).Select(row => grid[row][col]).ToArray();
        }

        private IntVar[] GetBlock(IntVar[][] grid, int blockRow, int blockCol)
        {
            return Enumerable.Range(0, 3)
                .SelectMany(i => Enumerable.Range(0, 3)
                .Select(j => grid[blockRow * 3 + i][blockCol * 3 + j]))
                .ToArray();
        }

       
	}
}
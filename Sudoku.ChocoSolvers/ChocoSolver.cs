using Sudoku.Shared;
using System;
using org.chocosolver.solver;
using org.chocosolver.solver.variables;

namespace Sudoku.ChocoSolvers
{
    public class ChocoSolver : ISudokuSolver
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

            // Contraintes pour les lignes et les colonnes
            for (int i = 0; i < 9; i++)
            {
                model.allDifferent(cellVariables[i]).post(); // Lignes
                model.allDifferent(GetColumn(cellVariables, i)).post(); // Colonnes
            }

            // Contraintes pour les blocs 3x3
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    model.allDifferent(GetBlock(cellVariables, blockRow, blockCol)).post();
                }
            }

            // Appliquer les valeurs initiales du Sudoku
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] != 0)
                    {
                        model.arithm(cellVariables[row][col], "=", s.Cells[row, col]).post();
                    }
                }
            }

            // Résolution du modèle
            var solver = model.getSolver();
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
using System;
using Sudoku.Shared;
using Google.OrTools.Sat;

namespace SudokuJulien;

public class ORToolsSimpleSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        // Initialisation du modèle de contraintes
        CpModel model = new CpModel();

        // Déclaration des variables : une matrice de 9x9 pour les cellules du Sudoku
        IntVar[,] cells = new IntVar[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                cells[i, j] = model.NewIntVar(1, 9, $"cell_{i}_{j}");
            }
        }

        // Contraintes de lignes : chaque ligne doit contenir des valeurs différentes
        for (int i = 0; i < 9; i++)
        {
            IntVar[] row = new IntVar[9];
            for (int j = 0; j < 9; j++)
            {
                row[j] = cells[i, j];
            }
            model.AddAllDifferent(row);
        }

        // Contraintes de colonnes : chaque colonne doit contenir des valeurs différentes
        for (int j = 0; j < 9; j++)
        {
            IntVar[] column = new IntVar[9];
            for (int i = 0; i < 9; i++)
            {
                column[i] = cells[i, j];
            }
            model.AddAllDifferent(column);
        }

        // Contraintes de sous-grilles (3x3) : chaque sous-grille doit contenir des valeurs différentes
        for (int startRow = 0; startRow < 9; startRow += 3)
        {
            for (int startCol = 0; startCol < 9; startCol += 3)
            {
                IntVar[] subGrid = new IntVar[9];
                int index = 0;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        subGrid[index++] = cells[startRow + i, startCol + j];
                    }
                }
                model.AddAllDifferent(subGrid);
            }
        }

        // Fixer les valeurs initiales de la grille
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s.Cells[i, j] != 0)
                {
                    model.Add(cells[i, j] == s.Cells[i, j]);
                }
            }
        }

        // Résolution du modèle
        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);

        // Si une solution est trouvée, mettre à jour la grille Sudoku
        if (status == CpSolverStatus.Feasible || status == CpSolverStatus.Optimal)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    s.Cells[i, j] = (int)solver.Value(cells[i, j]);
                }
            }
        }
        else
        {
            throw new Exception("Aucune solution trouvée pour ce Sudoku.");
        }

        return s;
    }
}

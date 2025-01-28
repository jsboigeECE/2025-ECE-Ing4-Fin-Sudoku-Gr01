using System;
using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;

namespace SudokuJulien;

public class ORToolsLegacyCPSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        // Initialisation du modèle de contraintes
        Solver solver = new Solver("Sudoku");

        // Déclaration des variables : une matrice de 9x9 pour les cellules du Sudoku
        IntVar[,] cells = new IntVar[9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                // Chaque cellule peut prendre une valeur entre 1 et 9
                cells[i, j] = solver.MakeIntVar(1, 9, $"cell_{i}_{j}");
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
            solver.Add(solver.MakeAllDifferent(row));
        }

        // Contraintes de colonnes : chaque colonne doit contenir des valeurs différentes
        for (int j = 0; j < 9; j++)
        {
            IntVar[] column = new IntVar[9];
            for (int i = 0; i < 9; i++)
            {
                column[i] = cells[i, j];
            }
            solver.Add(solver.MakeAllDifferent(column));
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
                solver.Add(solver.MakeAllDifferent(subGrid));
            }
        }

        // Fixer les valeurs initiales de la grille
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s.Cells[i, j] != 0)
                {
                    solver.Add(cells[i, j] == s.Cells[i, j]);
                }
            }
        }

        // Résolution du modèle
        DecisionBuilder db = solver.MakePhase(
            cells.Flatten(), // Transforme la matrice en tableau
            Solver.INT_VAR_SIMPLE, // Heuristique simple pour choisir les variables
            Solver.INT_VALUE_SIMPLE // Heuristique simple pour choisir les valeurs
        );

        // Lance la recherche
        solver.NewSearch(db);
        if (solver.NextSolution())
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    s.Cells[i, j] = (int)cells[i, j].Value();
                }
            }
        }
        else
        {
            throw new Exception("Aucune solution trouvée pour ce Sudoku.");
        }
        solver.EndSearch();

        return s;
    }
}

// Extension pour aplatir un tableau 2D en 1D
public static class ArrayExtensions
{
    public static IntVar[] Flatten(this IntVar[,] array)
    {
        return array.Cast<IntVar>().ToArray();
    }
}

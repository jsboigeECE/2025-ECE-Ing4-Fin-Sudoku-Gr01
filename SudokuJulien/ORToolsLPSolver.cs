using System;
using Sudoku.Shared;
using Google.OrTools.LinearSolver;

namespace SudokuJulien;

public class ORToolsLPSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        // Création du solveur LP
        Solver solver = Solver.CreateSolver("GLOP");
        if (solver == null)
        {
            throw new Exception("Le solveur linéaire OR-Tools n'a pas pu être initialisé.");
        }

        // Déclaration des variables : une matrice de 9x9x9 pour les valeurs possibles (LP utilise des variables continues)
        Variable[,,] cells = new Variable[9, 9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                for (int v = 0; v < 9; v++)
                {
                    cells[i, j, v] = solver.MakeNumVar(0, 1, $"cell_{i}_{j}_{v}");
                }
            }
        }

        // Contraintes : Chaque cellule contient exactement une valeur
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                LinearExpr sum = new LinearExpr();
                for (int v = 0; v < 9; v++)
                {
                    sum += cells[i, j, v];
                }
                solver.Add(sum == 1.0);
            }
        }

        // Contraintes : Chaque ligne contient chaque valeur une seule fois
        for (int i = 0; i < 9; i++)
        {
            for (int v = 0; v < 9; v++)
            {
                LinearExpr sum = new LinearExpr();
                for (int j = 0; j < 9; j++)
                {
                    sum += cells[i, j, v];
                }
                solver.Add(sum == 1.0);
            }
        }

        // Contraintes : Chaque colonne contient chaque valeur une seule fois
        for (int j = 0; j < 9; j++)
        {
            for (int v = 0; v < 9; v++)
            {
                LinearExpr sum = new LinearExpr();
                for (int i = 0; i < 9; i++)
                {
                    sum += cells[i, j, v];
                }
                solver.Add(sum == 1.0);
            }
        }

        // Contraintes : Chaque sous-grille 3x3 contient chaque valeur une seule fois
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                for (int v = 0; v < 9; v++)
                {
                    LinearExpr sum = new LinearExpr();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            sum += cells[3 * boxRow + i, 3 * boxCol + j, v];
                        }
                    }
                    solver.Add(sum == 1.0);
                }
            }
        }

        // Contraintes : Fixer les valeurs initiales de la grille
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s.Cells[i, j] != 0)
                {
                    solver.Add(cells[i, j, s.Cells[i, j] - 1] == 1.0);
                }
            }
        }

        // Objectif : Maximiser la somme des valeurs pour respecter les contraintes
        Objective objective = solver.Objective();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                for (int v = 0; v < 9; v++)
                {
                    objective.SetCoefficient(cells[i, j, v], 1.0);
                }
            }
        }
        objective.SetMaximization();

        // Résolution du problème LP
        Solver.ResultStatus status = solver.Solve();

        if (status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    for (int v = 0; v < 9; v++)
                    {
                        if (cells[i, j, v].SolutionValue() > 0.5)
                        {
                            s.Cells[i, j] = v + 1;
                        }
                    }
                }
            }
        }
        else
        {
            throw new Exception("Aucune solution trouvée pour ce Sudoku via LP.");
        }

        return s;
    }
}

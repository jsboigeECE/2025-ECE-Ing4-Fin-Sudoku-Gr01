using System;
using Sudoku.Shared;
using Google.OrTools.LinearSolver;

namespace SudokuJulien;

// Classes dérivées représentant différentes configurations de solveurs MIP
public class ORToolsMIPSimpleSolver : ORToolsMIPSolverBase
{
    public ORToolsMIPSimpleSolver() : base(MIPStrategy.Simple) { }
}

public class ORToolsMIPMinPreFilledSolver : ORToolsMIPSolverBase
{
    public ORToolsMIPMinPreFilledSolver() : base(MIPStrategy.MinimizePreFilled) { }
}

public class ORToolsMIPMaxSymmetrySolver : ORToolsMIPSolverBase
{
    public ORToolsMIPMaxSymmetrySolver() : base(MIPStrategy.MaximizeSymmetry) { }
}

// Classe de base mutualisant le code pour les solveurs MIP
public abstract class ORToolsMIPSolverBase : ISudokuSolver
{
    private readonly MIPStrategy _strategy;

    public ORToolsMIPSolverBase(MIPStrategy strategy)
    {
        _strategy = strategy;
    }

    public SudokuGrid Solve(SudokuGrid s)
    {
        // Création du solveur MIP
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
        {
            throw new Exception("Le solveur MIP OR-Tools n'a pas pu être initialisé.");
        }

        // Déclaration des variables : une matrice de 9x9x9 pour les valeurs possibles
        Variable[,,] cells = new Variable[9, 9, 9];
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                for (int v = 0; v < 9; v++)
                {
                    cells[i, j, v] = solver.MakeIntVar(0, 1, $"cell_{i}_{j}_{v}");
                }
            }
        }

        // Contraintes : chaque cellule doit contenir exactement une valeur entière
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

        // Contraintes : chaque ligne doit contenir chaque valeur une seule fois
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

        // Contraintes : chaque colonne doit contenir chaque valeur une seule fois
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

        // Contraintes : chaque sous-grille 3x3 doit contenir chaque valeur une seule fois
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

        // Contraintes : fixer les valeurs initiales de la grille
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


        // Configuration des stratégies d'optimisation
        Objective objective = solver.Objective();
        switch (_strategy)
        {
            case MIPStrategy.MinimizePreFilled:
                foreach (var cell in cells)
                {
                    objective.SetCoefficient(cell, 1.0);
                }
                objective.SetMinimization();
                break;

            case MIPStrategy.MaximizeSymmetry:
                foreach (var cell in cells)
                {
                    objective.SetCoefficient(cell, 1.0);
                }
                objective.SetMaximization();
                break;

            case MIPStrategy.Simple:
            default:
                objective.SetMaximization();
                break;
        }

        // Résolution
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
            throw new Exception("Aucune solution trouvée pour ce Sudoku via MIP.");
        }

        return s;
    }
}

// Enum pour les différentes stratégies MIP
public enum MIPStrategy
{
    Simple,
    MinimizePreFilled,
    MaximizeSymmetry
}

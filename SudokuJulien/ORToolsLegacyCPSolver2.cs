using System;
using Sudoku.Shared;
using Google.OrTools.ConstraintSolver;

namespace SudokuJulien;

public class ORToolsLegacyCPSolver2 : ISudokuSolver
{
    private readonly SearchStrategy _strategy;

    public ORToolsLegacyCPSolver2() : this(SearchStrategy.Random) {}

    public ORToolsLegacyCPSolver2(SearchStrategy strategy)
    {
        _strategy = strategy;
    }

    public SudokuGrid Solve(SudokuGrid s)
    {
        Solver solver = new Solver("Sudoku");

        // Déclaration des variables : une matrice jagged pour les cellules du Sudoku
        IntVar[][] cells = new IntVar[9][];
        for (int i = 0; i < 9; i++)
        {
            cells[i] = new IntVar[9];
            for (int j = 0; j < 9; j++)
            {
                cells[i][j] = solver.MakeIntVar(1, 9, $"cell_{i}_{j}");
            }
        }

        // Contraintes de lignes : chaque ligne doit contenir des valeurs différentes
        for (int i = 0; i < 9; i++)
        {
            solver.Add(solver.MakeAllDifferent(cells[i]));
        }

        // Contraintes de colonnes : chaque colonne doit contenir des valeurs différentes
        for (int j = 0; j < 9; j++)
        {
            IntVar[] column = new IntVar[9];
            for (int i = 0; i < 9; i++)
            {
                column[i] = cells[i][j];
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
                        subGrid[index++] = cells[startRow + i][startCol + j];
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
                    solver.Add(cells[i][j] == s.Cells[i, j]);
                }
            }
        }

        // Sélection de la stratégie de recherche
        DecisionBuilder db = GetDecisionBuilder(solver, cells);

        // Résolution
        solver.NewSearch(db);
        if (solver.NextSolution())
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    s.Cells[i, j] = (int)cells[i][j].Value();
                }
            }
        }
        else
        {
            throw new Exception("Aucune solution trouvée.");
        }
        solver.EndSearch();

        return s;
    }

    private DecisionBuilder GetDecisionBuilder(Solver solver, IntVar[][] cells)
    {
        switch (_strategy)
        {
            case SearchStrategy.FirstFail:
                return solver.MakePhase(
                    cells.Flatten(),
                    Solver.CHOOSE_MIN_SIZE_LOWEST_MIN, // Similaire à "FirstFail"
                    Solver.ASSIGN_MIN_VALUE);

            case SearchStrategy.Random:
                return solver.MakePhase(
                    cells.Flatten(),
                    Solver.CHOOSE_RANDOM, // Stratégie aléatoire
                    Solver.ASSIGN_RANDOM_VALUE);

            case SearchStrategy.Simple:
            default:
                return solver.MakePhase(
                    cells.Flatten(),
                    Solver.CHOOSE_FIRST_UNBOUND, // Choix par défaut
                    Solver.ASSIGN_MIN_VALUE);
        }
    }
}

public enum SearchStrategy
{
    Simple,
    FirstFail,
    Random
}

using System;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    /// <summary>
    /// Classe de base qui implémente la création des variables et les contraintes communes
    /// (domaine, lignes, colonnes, blocs 3x3).
    /// Les solveurs spécifiques implémentent l’application des valeurs fixes via
    /// la méthode abstraite AddInitialConstraints.
    /// </summary>
    public abstract class Z3SolverBase : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            using (var context = new Context())
            {
                Solver solver = context.MkSolver();
                IntExpr[,] cells = new IntExpr[9, 9];

                // Création des variables et contrainte sur le domaine [1,9]
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        cells[i, j] = (IntExpr)context.MkIntConst($"cell_{i}_{j}");
                        solver.Assert(
                            context.MkAnd(
                                context.MkLe(context.MkInt(1), cells[i, j]),
                                context.MkLe(cells[i, j], context.MkInt(9))
                            )
                        );
                    }
                }

                // Application des valeurs fixes de la grille (spécifique au solver)
                AddInitialConstraints(s, context, solver, cells);

                // Ajout des contraintes génériques : lignes, colonnes et blocs 3x3
                AddGenericConstraints(context, solver, cells);

                if (solver.Check() == Status.SATISFIABLE)
                {
                    Model model = solver.Model;
                    SudokuGrid solvedGrid = new SudokuGrid();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            solvedGrid.Cells[i, j] = ((IntNum)model.Evaluate(cells[i, j])).Int;
                        }
                    }
                    return solvedGrid;
                }
                else
                {
                    throw new Exception("Aucune solution trouvée");
                }
            }
        }

        /// <summary>
        /// Permet d’appliquer les contraintes liées aux valeurs fixes de la grille.
        /// Les classes dérivées doivent implémenter cette méthode.
        /// </summary>
        protected abstract void AddInitialConstraints(SudokuGrid s, Context context, Solver solver, IntExpr[,] cells);

        /// <summary>
        /// Ajoute les contraintes communes aux règles du Sudoku : lignes, colonnes, blocs.
        /// </summary>
        protected virtual void AddGenericConstraints(Context context, Solver solver, IntExpr[,] cells)
        {
            // Contrainte sur les lignes
            for (int i = 0; i < 9; i++)
            {
                Expr[] row = new Expr[9];
                for (int j = 0; j < 9; j++)
                {
                    row[j] = cells[i, j];
                }
                solver.Assert(context.MkDistinct(row));
            }

            // Contrainte sur les colonnes
            for (int j = 0; j < 9; j++)
            {
                Expr[] col = new Expr[9];
                for (int i = 0; i < 9; i++)
                {
                    col[i] = cells[i, j];
                }
                solver.Assert(context.MkDistinct(col));
            }

            // Contrainte sur les blocs 3x3
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    Expr[] block = new Expr[9];
                    int idx = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            block[idx++] = cells[blockRow * 3 + i, blockCol * 3 + j];
                        }
                    }
                    solver.Assert(context.MkDistinct(block));
                }
            }
        }
    }
}

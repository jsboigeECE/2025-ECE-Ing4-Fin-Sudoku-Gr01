using System;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    public class Z3ScopedSolver : Z3SolverBase
    {
        protected override void AddInitialConstraints(SudokuGrid s, Context context, Solver solver, IntExpr[,] cells)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        solver.Assert(context.MkEq(cells[i, j], context.MkInt(s.Cells[i, j])));
                    }
                }
            }
        }

        public override SudokuGrid Solve(SudokuGrid s)
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
                        solver.Assert(context.MkAnd(
                            context.MkLe(context.MkInt(1), cells[i, j]),
                            context.MkLe(cells[i, j], context.MkInt(9))
                        ));
                    }
                }

                // Ajout des contraintes initiales (valeurs fixes)
                AddInitialConstraints(s, context, solver, cells);

                // Ajout des contraintes génériques
                AddGenericConstraints(context, solver, cells);

                // Vérification de la solution de base
                if (solver.Check() != Status.SATISFIABLE)
                    throw new Exception("Aucune solution trouvée pour le Sudoku de base.");

                // Utilisation de Push pour ajouter une contrainte temporaire
                solver.Push();
                if (s.Cells[0, 0] == 0)
                {
                    // Contrainte temporaire : forcer la cellule [0,0] à être 1
                    solver.Assert(context.MkEq(cells[0, 0], context.MkInt(1)));
                }

                SudokuGrid variantGrid;
                if (solver.Check() == Status.SATISFIABLE)
                {
                    Model variantModel = solver.Model;
                    variantGrid = new SudokuGrid();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            variantGrid.Cells[i, j] = ((IntNum)variantModel.Evaluate(cells[i, j])).Int;
                        }
                    }
                }
                else
                {
                    // Si la variante n'est pas satisfaisable, on retire la contrainte temporaire
                    solver.Pop();
                    Model baseModel = solver.Model;
                    variantGrid = new SudokuGrid();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            variantGrid.Cells[i, j] = ((IntNum)baseModel.Evaluate(cells[i, j])).Int;
                        }
                    }
                    return variantGrid;
                }

                // Retrait du contexte temporaire
                solver.Pop();
                return variantGrid;
            }
        }
    }
}

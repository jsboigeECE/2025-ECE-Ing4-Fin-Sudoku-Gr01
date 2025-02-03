using System;
using System.Collections.Generic;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    public class Z3AssumptionsSolver : Z3SolverBase
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

                // Ajout des contraintes initiales (cellules déjà fixées)
                AddInitialConstraints(s, context, solver, cells);

                // Ajout des contraintes génériques
                AddGenericConstraints(context, solver, cells);

                // Préparation des hypothèses
                // Exemple : si la cellule [0,0] n'est pas fixée, on suppose qu'elle vaut 1.
                List<BoolExpr> assumptions = new List<BoolExpr>();
                if (s.Cells[0, 0] == 0)
                {
                    assumptions.Add(context.MkEq(cells[0, 0], context.MkInt(1)));
                }

                // Appel de Check avec le cast pour éviter l'ambiguïté
                if (solver.Check((IEnumerable<BoolExpr>)assumptions) == Status.SATISFIABLE)
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
                    throw new Exception("Aucune solution trouvée sous les hypothèses données.");
                }
            }
        }
    }
}

using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    public class SMTSimpleZ3 : Z3SolverBase
    {
        /// <summary>
        /// Pour chaque cellule préfixée (non nulle), on ajoute une contrainte d’égalité.
        /// </summary>
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
    }
}

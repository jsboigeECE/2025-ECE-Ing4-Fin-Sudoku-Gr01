using System;

using Microsoft.Z3;
using Sudoku.Shared;

namespace Sudoku.SMTZ3
{
    public abstract class Z3SolverBase : ISudokuSolver
    {
        protected Context ctx;
        protected Solver solver;
        protected IntExpr[,] cells;

        public SudokuGrid Solve(SudokuGrid s)
        {
            using (ctx = new Context())
            {
                solver = ctx.MkSolver();
                cells = new IntExpr[9, 9];

                InitializeCells();
                ApplyConstraints();
                ApplyInitialGrid(s);

                return SolveSudoku();
            }
        }

        protected void InitializeCells()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i, j] = (IntExpr)ctx.MkIntConst($"cell_{i}_{j}");
                    solver.Assert(ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), cells[i, j]), ctx.MkLe(cells[i, j], ctx.MkInt(9))));
                }
            }
        }

        protected void ApplyConstraints()
        {
            for (int i = 0; i < 9; i++)
            {
                solver.Assert(ctx.MkDistinct(cells[i, 0], cells[i, 1], cells[i, 2], cells[i, 3], cells[i, 4], cells[i, 5], cells[i, 6], cells[i, 7], cells[i, 8]));
                solver.Assert(ctx.MkDistinct(cells[0, i], cells[1, i], cells[2, i], cells[3, i], cells[4, i], cells[5, i], cells[6, i], cells[7, i], cells[8, i]));
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    solver.Assert(ctx.MkDistinct(
                        cells[i * 3, j * 3], cells[i * 3, j * 3 + 1], cells[i * 3, j * 3 + 2],
                        cells[i * 3 + 1, j * 3], cells[i * 3 + 1, j * 3 + 1], cells[i * 3 + 1, j * 3 + 2],
                        cells[i * 3 + 2, j * 3], cells[i * 3 + 2, j * 3 + 1], cells[i * 3 + 2, j * 3 + 2]
                    ));
                }
            }
        }

        protected void ApplyInitialGrid(SudokuGrid s)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        solver.Assert(ctx.MkEq(cells[i, j], ctx.MkInt(s.Cells[i, j])));
                    }
                }
            }
        }

        protected SudokuGrid SolveSudoku()
        {
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
                throw new Exception("No solution found");
            }
        }
    }
}

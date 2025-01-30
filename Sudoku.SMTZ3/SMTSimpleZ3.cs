using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    public class SMTSimpleZ3 : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            using (var context = new Context())
            {
                var solver = context.MkSolver();
                IntExpr[,] cells = new IntExpr[9, 9];

                // Create variables for each cell
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        cells[i, j] = (IntExpr)context.MkIntConst($"cell_{i}_{j}");
                        solver.Assert(context.MkAnd(context.MkLe(context.MkInt(1), cells[i, j]), context.MkLe(cells[i, j], context.MkInt(9))));
                    }
                }

                // Add constraints for the initial grid
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

                // Add constraints for rows, columns, and 3x3 subgrids
                for (int i = 0; i < 9; i++)
                {
                    solver.Assert(context.MkDistinct(cells[i, 0], cells[i, 1], cells[i, 2], cells[i, 3], cells[i, 4], cells[i, 5], cells[i, 6], cells[i, 7], cells[i, 8]));
                    solver.Assert(context.MkDistinct(cells[0, i], cells[1, i], cells[2, i], cells[3, i], cells[4, i], cells[5, i], cells[6, i], cells[7, i], cells[8, i]));
                }

                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        solver.Assert(context.MkDistinct(
                            cells[i * 3, j * 3], cells[i * 3, j * 3 + 1], cells[i * 3, j * 3 + 2],
                            cells[i * 3 + 1, j * 3], cells[i * 3 + 1, j * 3 + 1], cells[i * 3 + 1, j * 3 + 2],
                            cells[i * 3 + 2, j * 3], cells[i * 3 + 2, j * 3 + 1], cells[i * 3 + 2, j * 3 + 2]
                        ));
                    }
                }

                // Check if the solution exists
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
}
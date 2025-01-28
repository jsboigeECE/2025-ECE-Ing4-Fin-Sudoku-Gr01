using System.Runtime.InteropServices.JavaScript;
using Microsoft.Z3;
using Sudoku.Shared;

namespace Sudoku.Z3Solvers;

public class Z3SimpleSolver : ISudokuSolver
{


	public SudokuGrid Solve(SudokuGrid s)
	{
		return SolveWithZ3(s);
	}

	private SudokuGrid SolveWithZ3(SudokuGrid s)
	{
		Context ctx = new Context();
		// on va créer une matrice de 9 par 9 pour la grille du sudoku
		IntExpr[,] X = new IntExpr[9, 9];
		for (uint i = 0; i < 9; i++)
		{
			//X[i] = new IntExpr[9];
			for (uint j = 0; j < 9; j++)
				X[i, j] = (IntExpr)ctx.MkConst(ctx.MkSymbol("x_" + (i + 1) + "_" + (j + 1)), ctx.IntSort);
		}

		// chaques cases va contenir une valeur entre 1 et 9
		BoolExpr[][] cells_c = new BoolExpr[9][];
		for (uint i = 0; i < 9; i++)
		{
			cells_c[i] = new BoolExpr[9];
			for (uint j = 0; j < 9; j++)
				cells_c[i][j] = ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), X[i, j]),
										  ctx.MkLe(X[i, j], ctx.MkInt(9)));
		}

		// chaque ligne et chaque colonne va contenir un seul et même chiffre une seule fois
		BoolExpr[] rows_c = new BoolExpr[9];
		BoolExpr[] cols_c = new BoolExpr[9];
		for (uint i = 0; i < 9; i++)
		{
			//Ligne
			var rowCells = Enumerable.Range(0, 9).Select(j => (Expr)X[i, j]).ToArray();
			rows_c[i] = ctx.MkDistinct(rowCells);
			//Colonne
			var columnCells = Enumerable.Range(0, 9).Select(j => (Expr)X[j, i]).ToArray();
			cols_c[i] = ctx.MkDistinct(columnCells);

		}


		// chaque carré de 3 par 3 contient un seul et même chiffre une seule fois 
		BoolExpr[][] sq_c = new BoolExpr[3][];
		for (uint i0 = 0; i0 < 3; i0++)
		{
			sq_c[i0] = new BoolExpr[3];
			for (uint j0 = 0; j0 < 3; j0++)
			{
				Expr[] square = new Expr[9];
				for (uint i = 0; i < 3; i++)
					for (uint j = 0; j < 3; j++)
						square[3 * i + j] = X[3 * i0 + i, 3 * j0 + j];
				sq_c[i0][j0] = ctx.MkDistinct(square);
			}
		}

		BoolExpr sudoku_c = ctx.MkTrue();
		foreach (BoolExpr[] t in cells_c)
			sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);
		sudoku_c = ctx.MkAnd(ctx.MkAnd(rows_c), sudoku_c);
		sudoku_c = ctx.MkAnd(ctx.MkAnd(cols_c), sudoku_c);
		foreach (BoolExpr[] t in sq_c)
			sudoku_c = ctx.MkAnd(ctx.MkAnd(t), sudoku_c);

		// on utilise les 0 pour signifier les cases vides
		BoolExpr instance_c = ctx.MkTrue();
		for (uint i = 0; i < 9; i++)
			for (uint j = 0; j < 9; j++)
				instance_c = ctx.MkAnd(instance_c, (BoolExpr)ctx.MkITE(ctx.MkEq(ctx.MkInt(s.Cells[i, j]), ctx.MkInt(0)), ctx.MkTrue(), ctx.MkEq(X[i,j], ctx.MkInt(s.Cells[i, j]))));

		Solver z3Solver = ctx.MkSolver();
		z3Solver.Assert(sudoku_c);
		z3Solver.Assert(instance_c);

		if (z3Solver.Check() == Status.SATISFIABLE)
		{
			Model m = z3Solver.Model;
			int[,] solution = new int[9, 9];

			for (uint i = 0; i < 9; i++)
			{
				for (uint j = 0; j < 9; j++)
				{
					Expr cellValue = m.Evaluate(X[i, j]);

					if (cellValue is IntNum)
						solution[i, j] = (int)(cellValue as IntNum).Int;
					else
					{

						Console.WriteLine("Unexpected non-integer value in the solution.");
						ctx.Dispose(); // Libérer les ressources de Z3
						throw new Exception("Unexpected non-integer value in the solution");
					}
				}
			}

			ctx.Dispose(); // Libérer les ressources de Z3

			// Mettez à jour la grille SudokuGrid avec la solution
			for (uint i = 0; i < 9; i++)
				for (uint j = 0; j < 9; j++)
					s.Cells[i, j] = solution[i, j];

			return s;
		}
		else
		{
			// Ajustez le comportement lorsque la grille n'est pas résolue.
			Console.WriteLine("Failed to solve sudoku");
			ctx.Dispose(); // Libérer les ressources de Z3
			throw new Exception("Sudoku not solvable");
		}
	}
}

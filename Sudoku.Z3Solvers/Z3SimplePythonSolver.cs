using System;
using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.Z3Solvers;

public class Z3SimplePythonSolver : PythonSolverBase

{
	public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
	{
		//System.Diagnostics.Debugger.Break();

		//For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
		//using (Py.GIL())
		//{
		// create a Python scope
		using (PyModule scope = Py.CreateScope())
		{

			// Conversion directe sans NumPy
			var pyCells = new PyList();
			for (int i = 0; i < 9; i++)
			{
				var row = new PyList();
				for (int j = 0; j < 9; j++)
					row.Append(s.Cells[i, j].ToPython());
				pyCells.Append(row);
			}

			scope.Set("instance", pyCells);

			// run the Python script
			string code = System.IO.File.ReadAllText("z3solver.py");


			scope.Exec(code);

			dynamic result = scope.Get("result");
			var managedResult = new int[9, 9];
			for (int i = 0; i < 9; i++)
			for (int j = 0; j < 9; j++)
				managedResult[i, j] = (int)result[i][j];

			return new SudokuGrid { Cells = managedResult };
		}
		//}

	}




	protected override void InitializePythonComponents()
	{
		//declare your pip packages here
		InstallPipModule("numpy");
		InstallPipModule("z3-solver==4.12.2.0");
		base.InitializePythonComponents();
	}

}
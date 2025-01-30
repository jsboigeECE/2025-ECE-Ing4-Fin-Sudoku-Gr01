using org.chocosolver.solver;
using org.chocosolver.solver.constraints;
using org.chocosolver.solver.search.strategy;
using org.chocosolver.solver.variables;
using Sudoku.Shared;

namespace Sudoku.ChocoSolvers;

public class ChocoDomOverWDegRefSearchSolver : ChocoSimpleSolver
{
	public override Solver GetSolver(Model model, IntVar[][] cellVariables, List<Constraint> constraints)
	{
		var solver = model.getSolver();
			
		foreach (var constraint in constraints)
		{
			constraint.post();
		}
		// cf https://choco-solver.org/docs/solving/strategies/

		solver.setSearch(Search.domOverWDegRefSearch(cellVariables.Flatten()));

		return solver;
	}
}
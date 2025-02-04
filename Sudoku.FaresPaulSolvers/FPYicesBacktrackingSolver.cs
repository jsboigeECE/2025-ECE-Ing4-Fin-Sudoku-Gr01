using System;

namespace Sudoku.FaresPaulSolvers;

public class YicesSolverBacktrack : BaseYicesSolver
{
    protected override string GetHeuristicConfig()
    {
        return "(set-option :random-seed 42)\n(set-option :branching-effort 3)\n";
    }
}
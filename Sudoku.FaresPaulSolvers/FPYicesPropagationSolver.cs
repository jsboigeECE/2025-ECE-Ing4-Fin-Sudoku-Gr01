using System;

namespace Sudoku.FaresPaulSolvers;

public class YicesSolverPropagation : BaseYicesSolver
{
    protected override string GetHeuristicConfig()
    {
        return "(set-option :var-elim true)\n(set-option :eager-arith true)\n";
    }
}
using System;
using System.Linq;
using DlxLib;
using Sudoku.Shared;

namespace Sudoku.DancingLinksTenzin
{
    public class DancingLinksSolverSimple : DancingLinksSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            var (matrix, constraintsMap) = ConvertToExactCoverMatrix(s);
            var solver = new Dlx();
            var solution = solver.Solve(matrix, row => row, row => row).FirstOrDefault();
            
            if (solution != null)
            {
                var solvedGrid = ConvertToSudokuGrid(solution.RowIndexes, constraintsMap, s);
                Console.WriteLine("Solution trouvée par DLX:");
                Console.WriteLine(solvedGrid.ToString());
                return solvedGrid;
            }
            
            Console.WriteLine("Aucune solution trouvée");
            return s.CloneSudoku();
        }
        
}
}

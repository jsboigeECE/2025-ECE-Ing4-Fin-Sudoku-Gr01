using Sudoku.Shared;
using Sudoku.Backtracking;
using System;

namespace Sudoku.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Créer un générateur de Sudoku
            SudokuGenerator generator = new SudokuGenerator();

            // Générer une grille de Sudoku aléatoire
            SudokuGrid sudokuGrid = generator.GenerateSudoku();

              if (sudokuGrid == null)
            {
                Console.WriteLine("Impossible de générer un Sudoku valide.");
                return;
            }

            // Afficher la grille avant la résolution
            Console.WriteLine("Sudoku généré :");
            sudokuGrid.Afficher();

            // Instancier le solveur BacktrackingDotNetSolver
            var solver = new BacktrackingDotNetSolver();

            // Résoudre le Sudoku
            //bool solved = solver.Solve(sudokuGrid);
            SudokuGrid solvedGrid = solver.Solve(sudokuGrid);
        if (solvedGrid != null)
        {
            // Afficher la grille après résolution
            Console.WriteLine("\nSudoku résolu :");
            solvedGrid.Afficher();
        }
        else
        {
            Console.WriteLine("\nLe Sudoku ne peut pas être résolu.");
        }


            //if (solved)
            if (solvedGrid != null)

            {
                // Afficher la grille après résolution
                Console.WriteLine("\nSudoku résolu :");
                sudokuGrid.Afficher();
            }
            else
            {
                Console.WriteLine("\nLe Sudoku ne peut pas être résolu.");
            }
        }
    }
}

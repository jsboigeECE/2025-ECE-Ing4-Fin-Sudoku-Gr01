using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Créer un générateur de Sudoku
            SudokuGenerator generator = new SudokuGenerator();

            // Générer une grille de Sudoku aléatoire
            SudokuGrid generatedSudoku = generator.GenerateSudoku();

            if (generatedSudoku == null)
            {
                Console.WriteLine("❌ Impossible de générer un Sudoku valide.");
                return;
            }

            // Afficher la grille avant la résolution
            Console.WriteLine("🎲 Sudoku généré :");
            generatedSudoku.Afficher();

            
            // Résoudre le Sudoku avec la méthode AIMA CSP (avec AC-3 et backtracking)
            SudokuSolverAima solver = new SudokuSolverAima();
            bool isSolved = solver.Solve(generatedSudoku);

            Console.WriteLine("\n" + new string('-', 30)); // Séparateur visuel

            if (isSolved)
            {
                Console.WriteLine("✅ Sudoku résolu :");
                generatedSudoku.Afficher();
            }
            else
            {
                Console.WriteLine("❌ Aucune solution trouvée.");
            }

            Console.WriteLine("\nAppuyez sur Entrée pour quitter...");
            Console.ReadLine();
        }
    }

    public class SudokuSolverAima
    {
        public bool Solve(SudokuGrid grid)
        {
            // Applique l'arc-consistency initiale
            ArcConsistency(grid);
            return Backtrack(grid);
        }

        private bool Backtrack(SudokuGrid grid)
        {
            // Recherche la prochaine case non assignée
            var emptyCell = GetEmptyCell(grid);

            if (emptyCell == null)
                return true; // Sudoku résolu

            int row = emptyCell.Value.row;
            int col = emptyCell.Value.column;

            // Récupérer les valeurs possibles pour cette case
            var availableNumbers = grid.GetAvailableNumbers(row, col);

            foreach (var num in availableNumbers)
            {
                // Tentative d'affectation du numéro
                grid.Cells[row, col] = num;

                // Vérifie la validité de l'affectation et résout récursivement
                if (IsConsistent(grid, row, col) && Backtrack(grid))
                {
                    return true;
                }

                // Échec de l'affectation, retour en arrière
                grid.Cells[row, col] = 0;
            }

            return false; // Retour en arrière si aucune solution trouvée
        }

        private bool IsConsistent(SudokuGrid grid, int row, int col)
        {
            // Vérifie les lignes, colonnes et blocs pour les contraintes de consistance
            var rowValues = grid.GetRow(row);
            var colValues = grid.GetColomn(col);
            var boxValues = grid.GetBoxValues(row, col); 


            return !(rowValues.Contains(grid.Cells[row, col]) || 
                     colValues.Contains(grid.Cells[row, col]) || 
                     boxValues.Contains(grid.Cells[row, col]));
        }

        private (int row, int column)? GetEmptyCell(SudokuGrid grid)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        return (row, col); // Retourne la première cellule vide
                    }
                }
            }

            return null; // Aucune cellule vide trouvée
        }

        private void ArcConsistency(SudokuGrid grid)
        {
            bool revised;
            do
            {
                revised = false;

                // Vérification des contraintes de tous les arcs (cellules voisines)
                foreach (var (r, c) in SudokuGrid.NeighbourIndices.SelectMany(i => SudokuGrid.NeighbourIndices.Select(j => (i, j))))

                {
                    foreach (var neighbour in SudokuGrid.CellNeighbours[r][c])

                    {
                        if (Revise(grid, r, c, neighbour.row, neighbour.column))
                        {
                            revised = true;
                        }
                    }
                }

            } while (revised);
        }

        private bool Revise(SudokuGrid grid, int row1, int col1, int row2, int col2)
        {
            bool revised = false;
            var domain1 = new HashSet<int>(grid.GetAvailableNumbers(row1, col1));
            var domain2 = new HashSet<int>(grid.GetAvailableNumbers(row2, col2));

            // Vérifie si la cellule a plusieurs valeurs possibles
            foreach (var num in domain1.ToList())
            {
                // Si le numéro n'a pas de valeur compatible dans l'autre domaine, on le retire
                if (domain2.All(d => d != num))
                {
                    domain1.Remove(num);
                    revised = true;
                }
            }

            return revised;
        }
    }
}

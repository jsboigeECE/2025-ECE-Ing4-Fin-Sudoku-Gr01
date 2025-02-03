using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.AIMASolvers
{
    public class AIMAsimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            var solvedGrid = s.CloneSudoku();

            // Définition formelle du CSP
            var X = GetVariables(solvedGrid); // X : Ensemble des cellules vides
            var D = GetDomains(solvedGrid, X); // D : Domaine de chaque cellule
            var C = GetConstraints(); // C : Ensemble de contraintes

            // Appliquer AC-3 avant de commencer la recherche
            AC3(solvedGrid, X, D, C);

            if (BacktrackingSearch(solvedGrid, X, D, C))
            {
                return solvedGrid;
            }
            return s; // Retourne la grille originale si aucune solution trouvée
        }

        // X : Ensemble des variables (cellules vides)
        private List<(int row, int col)> GetVariables(SudokuGrid grid)
        {
            var variables = new List<(int row, int col)>();
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (grid.Cells[row, col] == 0)
                    {
                        variables.Add((row, col));
                    }
                }
            }
            return variables;
        }

        // D : Ensemble des domaines de valeurs possibles pour chaque cellule vide
        private Dictionary<(int row, int col), List<int>> GetDomains(SudokuGrid grid, List<(int row, int col)> variables)
        {
            var domains = new Dictionary<(int row, int col), List<int>>();
            foreach (var (row, col) in variables)
            {
                domains[(row, col)] = grid.GetAvailableNumbers(row, col).ToList();
            }
            return domains;
        }

        // C : Ensemble des contraintes (Sudoku : pas de doublons sur ligne, colonne, bloc)
        private List<Func<SudokuGrid, (int, int), int, bool>> GetConstraints()
        {
            return new List<Func<SudokuGrid, (int, int), int, bool>> { IsValid };
        }

        // Fonction de consistance d'arc (AC-3)
        private bool AC3(SudokuGrid grid, List<(int row, int col)> X, Dictionary<(int row, int col), List<int>> D,
                         List<Func<SudokuGrid, (int, int), int, bool>> C)
        {
            var queue = new Queue<(int row, int col)>();

            // Initialiser la queue avec toutes les arêtes (les cellules et leurs voisins)
            foreach (var (row, col) in X)
            {
                foreach (var (r, c) in SudokuGrid.CellNeighbours[row][col])
                {
                    queue.Enqueue((row, col));
                }
            }

            while (queue.Count > 0)
            {
                var (row, col) = queue.Dequeue();
                bool revised = false;

                foreach (var num in D[(row, col)].ToList())
                {
                    if (!IsValid(grid, (row, col), num))
                    {
                        D[(row, col)].Remove(num);
                        revised = true;
                    }
                }

                if (revised)
                {
                    foreach (var (r, c) in SudokuGrid.CellNeighbours[row][col])
                    {
                        queue.Enqueue((r, c)); // Ajouter les voisins à la queue
                    }
                }
            }

            return true;
        }

        private bool BacktrackingSearch(SudokuGrid grid, List<(int row, int col)> X,
            Dictionary<(int row, int col), List<int>> D, List<Func<SudokuGrid, (int, int), int, bool>> C)
        {
            if (X.Count == 0)
            {
                return true; // Toutes les cellules sont assignées, solution trouvée
            }

            var (row, col) = X[0]; // Sélection d’une variable (MRV pourrait améliorer)
            X.RemoveAt(0);

            foreach (var num in OrderDomainValues(D[(row, col)]))
            {
                if (C.All(constraint => constraint(grid, (row, col), num))) // Vérifie les contraintes
                {
                    grid.Cells[row, col] = num;
                    if (BacktrackingSearch(grid, X, D, C))
                    {
                        return true;
                    }
                    grid.Cells[row, col] = 0; // Annule la tentative
                }
            }

            X.Insert(0, (row, col)); // Remettre la variable si aucun choix n’a fonctionné
            return false;
        }

        // Tri aléatoire du domaine (peut être remplacé par LCV)
        private IEnumerable<int> OrderDomainValues(List<int> domain)
        {
            return domain.OrderBy(_ => Guid.NewGuid());
        }

        // Vérifie les contraintes du Sudoku
        private bool IsValid(SudokuGrid grid, (int row, int col) cell, int num)
        {
            foreach (var (r, c) in SudokuGrid.CellNeighbours[cell.row][cell.col])
            {
                if (grid.Cells[r, c] == num)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

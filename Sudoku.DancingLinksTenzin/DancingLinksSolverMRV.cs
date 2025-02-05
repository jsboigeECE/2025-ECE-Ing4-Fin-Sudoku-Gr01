using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;
using DlxLib;

namespace Sudoku.DancingLinksTenzin
{
    // Cette classe étend DancingLinksSolverBase et utilise une heuristique MRV
    public class DancingLinksSolverMRV : DancingLinksSolverBase
    {
        public override SudokuGrid Solve(SudokuGrid s)
        {
            // Conversion du sudoku en matrice d'exact cover ainsi qu'une map des indices originaux
            var (matrix, constraintsMap) = ConvertToExactCoverMatrix(s);
            // Création d'une liste des indices originaux pour chaque ligne de la matrice
            var rowIndices = Enumerable.Range(0, matrix.Count).ToList();

            // Appel de la méthode récursive avec suivi des indices originaux
            var solutionRows = SolveWithMRV(matrix, rowIndices);
            if (solutionRows != null)
            {
                // Reconstruction de la grille resolue à partir des indices originaux de solution
                var solvedGrid = ConvertToSudokuGrid(solutionRows, constraintsMap, s);
                Console.WriteLine("Solution trouvée avec MRV:");
                Console.WriteLine(solvedGrid.ToString());
                return solvedGrid;
            }
            Console.WriteLine("Aucune solution trouvée.");
            return s.CloneSudoku();
        }

        // Méthode récursive qui conserve une trace des indices originaux
        private List<int> SolveWithMRV(List<int[]> matrix, List<int> rowIndices)
        {
            // Condition de terminaison : si la matrice est vide, toutes les contraintes sont satisfaites
            if (!matrix.Any() || matrix[0].Length == 0)
                return new List<int>();

            // Selection de la colonne avec le moins de 1 (MRV)
            int col = SelectColumnWithMRV(matrix);

            // Récupération des lignes candidates ayant un 1 dans la colonne sélectionnée
            var candidateRows = matrix
                .Select((row, index) => new { Row = row, LocalIndex = index })
                .Where(item => item.Row[col] == 1)
                .ToList();

            if (!candidateRows.Any())
                return null; // Retour en arrière

            // Parcours des candidats
            foreach (var candidate in candidateRows)
            {
                // Filtrage de la matrice et mise à jour des indices originaux
                var filtered = FilterMatrix(matrix, rowIndices, candidate.LocalIndex);
                var subSolution = SolveWithMRV(filtered.matrix, filtered.rowIndices);
                if (subSolution != null)
                {
                    // Remonter l'indice original pour la solution
                    subSolution.Insert(0, rowIndices[candidate.LocalIndex]);
                    return subSolution;
                }
            }
            return null;
        }

        // Implémentation de l'heuristique MRV pour choisir la colonne avec le moins de 1
        private int SelectColumnWithMRV(List<int[]> matrix)
        {
            int bestColumn = -1;
            int minCount = int.MaxValue;
            int totalColumns = matrix[0].Length;
            for (int c = 0; c < totalColumns; c++)
            {
                int count = 0;
                foreach (var row in matrix)
                {
                    if (row[c] == 1)
                        count++;
                }
                if (count > 0 && count < minCount)
                {
                    minCount = count;
                    bestColumn = c;
                }
            }
            return bestColumn;
        }

        // Filtre la matrice en retirant les lignes incompatibles avec la ligne choisie
        // et met à jour la liste des indices originaux correspondants.
        private (List<int[]> matrix, List<int> rowIndices) FilterMatrix(List<int[]> matrix, List<int> rowIndices, int chosenRowIndex)
        {
            var chosenRow = matrix[chosenRowIndex];
            var columnsToRemove = new HashSet<int>(
                Enumerable.Range(0, chosenRow.Length).Where(i => chosenRow[i] == 1)
            );

            var newMatrix = new List<int[]>();
            var newRowIndices = new List<int>();

            for (int r = 0; r < matrix.Count; r++)
            {
                if (r == chosenRowIndex)
                    continue;

                bool conflict = false;
                for (int c = 0; c < chosenRow.Length; c++)
                {
                    if (chosenRow[c] == 1 && matrix[r][c] == 1)
                    {
                        conflict = true;
                        break;
                    }
                }
                if (conflict)
                    continue;

                // Retirer les colonnes correspondantes aux contraintes déjà satisfaites
                var newRow = new List<int>();
                for (int c = 0; c < matrix[r].Length; c++)
                {
                    if (!columnsToRemove.Contains(c))
                        newRow.Add(matrix[r][c]);
                }
                newMatrix.Add(newRow.ToArray());
                newRowIndices.Add(rowIndices[r]);
            }

            return (newMatrix, newRowIndices);
        }
    }
}
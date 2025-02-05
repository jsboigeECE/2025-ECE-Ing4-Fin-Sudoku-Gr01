using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace GraphColoring
{
    // Implémentation du solveur de Sudoku utilisant l'algorithme DSatur pour la coloration des graphes.
    public class DSaturSolver : ISudokuSolver
    {
        // Graph des connexions utilisé pour gérer les relations entre les cellules du Sudoku.
        private SudokuConnections sudokuGraph;
        // Matrice pour mapper les positions de la grille de Sudoku aux noeuds du graph.
        private int[][] mappedGrid;
        // Limite maximale de tentatives pour éviter les boucles infinies.
        private const int MaxAttempts = 1000000;

        // Constructeur qui initialise le graphe et la matrice de mappage.
        public DSaturSolver()
        {
            sudokuGraph = new SudokuConnections();
            mappedGrid = GetMappedMatrix();
        }

        // Méthode pour résoudre le Sudoku en utilisant l'approche de coloration de graphe DSatur.
        public SudokuGrid Solve(SudokuGrid s)
        {
            var board = new int[9, 9]; // Stockage temporaire pour la grille de Sudoku.
            // Copie les valeurs initiales du Sudoku dans le tableau 'board'.
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    board[i, j] = s.Cells[i, j];

            // Tente de résoudre le Sudoku en utilisant le backtracking et DSatur.
            if (SolveDSaturWithBacktracking(board))
            {
                // Si résolu, met à jour la grille de Sudoku originale avec la solution.
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                        s.Cells[i, j] = board[i, j];
            }

            return s; // Retourne la grille de Sudoku potentiellement résolue.
        }

        // Utilise l'algorithme DSatur combiné avec le backtracking pour résoudre le Sudoku.
        private bool SolveDSaturWithBacktracking(int[,] board)
        {
            var (row, col) = FindEmptyCell(board); // Trouve la prochaine cellule vide avec la plus haute saturation.
            if (row == -1 && col == -1) return true; // Si aucune cellule vide, le Sudoku est résolu.

            // Obtient toutes les valeurs possibles pouvant être placées dans la cellule vide.
            var possibleValues = GetPossibleValues(board, row, col);
            foreach (var value in possibleValues)
            {
                // Vérifie si placer la valeur est sûr selon les règles du Sudoku.
                if (IsSafe(board, row, col, value))
                {
                    board[row, col] = value; // Place la valeur.

                    // Tente de résoudre récursivement le reste de la grille.
                    if (SolveDSaturWithBacktracking(board))
                        return true;
                    
                    board[row, col] = 0; // Annule le mouvement (backtrack).
                }
            }
            return false; // Retourne faux si aucune solution trouvée.
        }

        // Trouve la prochaine cellule vide dans la grille avec le degré de saturation le plus élevé.
        private (int row, int col) FindEmptyCell(int[,] board)
        {
            int maxSaturation = -1;
            int selectedRow = -1;
            int selectedCol = -1;

            // Évalue chaque cellule dans la grille de Sudoku.
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] == 0) // Considère uniquement les cellules vides.
                    {
                        int saturation = CalculateSaturation(board, row, col); // Calcule combien de chiffres différents sont adjacents.
                        if (saturation > maxSaturation)
                        {
                            maxSaturation = saturation;
                            selectedRow = row;
                            selectedCol = col;
                        }
                    }
                }
            }
            return (selectedRow, selectedCol); // Retourne la cellule vide la plus contrainte.
        }

        // Calcule la saturation (nombre de chiffres adjacents différents) pour une cellule donnée.
        private int CalculateSaturation(int[,] board, int row, int col)
        {
            var usedValues = new HashSet<int>(); // Stocke les chiffres uniques adjacents à la cellule.
            
            // Vérifie toutes les cellules dans la même ligne, colonne et bloc 3x3.
            for (int c = 0; c < 9; c++)
                if (board[row, c] != 0)
                    usedValues.Add(board[row, c]);

            for (int r = 0; r < 9; r++)
                if (board[r, col] != 0)
                    usedValues.Add(board[r, col]);

            int boxRow = row - row % 3;
            int boxCol = col - col % 3;
            for (int r = boxRow; r < boxRow + 3; r++)
                for (int c = boxCol; c < boxCol + 3; c++)
                    if (board[r, c] != 0)
                        usedValues.Add(board[r, c]);

            return usedValues.Count; // Retourne le nombre de chiffres adjacents uniques.
        }

        // Retourne une liste de valeurs possibles pour une cellule en respectant les règles du Sudoku.
        private List<int> GetPossibleValues(int[,] board, int row, int col)
        {
            var used = new bool[10]; // Trace les numéros déjà utilisés dans la ligne, colonne et bloc.
            
            // Marque les numéros présents dans la même ligne, colonne et bloc.
            for (int c = 0; c < 9; c++)
                if (board[row, c] != 0)
                    used[board[row, c]] = true;

            for (int r = 0; r < 9; r++)
                if (board[r, col] != 0)
                    used[board[r, col]] = true;

            int boxRow = row - row % 3;
            int boxCol = col - col % 3;
            for (int r = boxRow; r < boxRow + 3; r++)
                for (int c = boxCol; c < boxCol + 3; c++)
                    if (board[r, c] != 0)
                        used[board[r, c]] = true;

            var possibleValues = new List<int>(); // Liste pour stocker toutes les valeurs possibles pour la cellule.
            for (int i = 1; i <= 9; i++)
                if (!used[i])
                    possibleValues.Add(i); // Ajoute le numéro s'il n'est pas utilisé.

            return possibleValues; // Retourne la liste des numéros possibles.
        }

        // Vérifie si placer un numéro dans une cellule spécifiée viole les règles du Sudoku.
        private bool IsSafe(int[,] board, int row, int col, int num)
        {
            // Vérifie que le numéro n'est pas présent dans la même ligne, colonne ou bloc 3x3.
            for (int c = 0; c < 9; c++)
                if (board[row, c] == num)
                    return false;

            for (int r = 0; r < 9; r++)
                if (board[r, col] == num)
                    return false;

            int boxRow = row - row % 3;
            int boxCol = col - col % 3;
            for (int r = boxRow; r < boxRow + 3; r++)
                for (int c = boxCol; c < boxCol + 3; c++)
                    if (board[r, c] == num)
                        return false;

            return true; // Retourne vrai si aucun conflit n'est trouvé.
        }

        // Génère une matrice pour mapper les cellules de la grille de Sudoku aux noeuds du graph.
        private int[][] GetMappedMatrix()
        {
            var matrix = new int[9][];
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int count = 1;
            for (int rows = 0; rows < 9; rows++)
                for (int cols = 0; cols < 9; cols++)
                {
                    matrix[rows][cols] = count++; // Attribue un index unique à chaque cellule.
                }
            return matrix;
        }
    }
}

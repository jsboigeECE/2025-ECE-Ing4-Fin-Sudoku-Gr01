using Sudoku.Shared;
using System;
using System.Collections.Generic;

public class SudokuGenerator
{
    private Random _random = new Random();

    // Génère un Sudoku valide
    public SudokuGrid GenerateSudoku()
    {
        int[,] grid = new int[9, 9];
        if (Generate(grid))
        {
            // Créer des trous après avoir généré un Sudoku valide
            CreateHoles(grid, 40);  // 40 est un exemple, tu peux ajuster ce nombre selon ton besoin

            return new SudokuGrid { Cells = grid };
        }

        return null;
    }

    // Méthode pour remplir la grille de Sudoku
    private bool Generate(int[,] grid)
    {
        // Tente de remplir la grille avec des chiffres valides
        return FillGrid(grid, 0, 0);
    }

    private bool FillGrid(int[,] grid, int row, int col)
    {
        // Si nous avons rempli toutes les lignes
        if (row == 9)
            return true;

        // Si nous avons rempli toutes les colonnes, passer à la ligne suivante
        if (col == 9)
            return FillGrid(grid, row + 1, 0);

        // Si la case est déjà remplie, passer à la suivante
        if (grid[row, col] != 0)
            return FillGrid(grid, row, col + 1);

        // Essayer chaque chiffre possible de 1 à 9
        var availableNumbers = GetAvailableNumbers(grid, row, col);
        foreach (var num in availableNumbers)
        {
            grid[row, col] = num;
            if (FillGrid(grid, row, col + 1))
                return true;
            grid[row, col] = 0; // Si ce chiffre ne fonctionne pas, essayer le suivant
        }

        return false;
    }

    // Retourne les numéros disponibles pour une case donnée
    private int[] GetAvailableNumbers(int[,] grid, int row, int col)
    {
        bool[] used = new bool[9];

        // Vérifier les voisins dans la même ligne
        for (int c = 0; c < 9; c++)
            if (grid[row, c] != 0)
                used[grid[row, c] - 1] = true;

        // Vérifier les voisins dans la même colonne
        for (int r = 0; r < 9; r++)
            if (grid[r, col] != 0)
                used[grid[r, col] - 1] = true;

        // Vérifier les voisins dans la même boîte
        int boxRow = (row / 3) * 3;
        int boxCol = (col / 3) * 3;
        for (int r = boxRow; r < boxRow + 3; r++)
        {
            for (int c = boxCol; c < boxCol + 3; c++)
            {
                if (grid[r, c] != 0)
                    used[grid[r, c] - 1] = true;
            }
        }

        // Retourner les chiffres disponibles
        var available = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (!used[i])
                available.Add(i + 1);
        }

        return available.ToArray();
    }

    // Cette méthode crée des trous dans la grille en retirant des valeurs.
    private void CreateHoles(int[,] grid, int numberOfHoles)
    {
        Random rand = new Random();
        int count = 0;

        while (count < numberOfHoles)
        {
            int row = rand.Next(0, 9);
            int col = rand.Next(0, 9);

            if (grid[row, col] != 0)  // Si la case n'est pas déjà vide
            {
                grid[row, col] = 0;  // Créer un trou
                count++;
            }
        }
    }
}

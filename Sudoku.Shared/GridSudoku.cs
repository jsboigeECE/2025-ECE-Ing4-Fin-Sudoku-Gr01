using System;

namespace Sudoku.Shared
{
    public class GridSudoku
    {
        public int[,] Cells { get; set; }

        public GridSudoku()
        {
            Cells = new int[9, 9]; // Initialise la grille de Sudoku (9x9)
        }

        // Méthode pour afficher la grille
        public void Display()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(Cells[i, j] == 0 ? "." : Cells[i, j].ToString());
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }
    }
}

using Sudoku.Shared; // Assurez-vous d'importer la classe SudokuGrid

public class SudokuCSP
{
    public SudokuGrid Grid { get; set; }

    // Constructeur
    public SudokuCSP(SudokuGrid grid)
    {
        Grid = grid;
    }

    // Ajoute ici les méthodes pour résoudre le Sudoku
    public bool Solve()
{
    // Trouver une case vide
    var emptyCell = FindEmptyCell();
    if (emptyCell == null)
    {
        return true; // Si toutes les cases sont remplies, Sudoku résolu
    }

    int row = emptyCell.Item1;
    int col = emptyCell.Item2;

    // Essayer chaque nombre possible (de 1 à 9)
    foreach (var number in Grid.GetAvailableNumbers(row, col))
    {
        if (IsValidAssignment(row, col, number))
        {
            Grid.Cells[row, col] = number;
            if (Solve())
            {
                return true;
            }
            Grid.Cells[row, col] = 0; // Réinitialiser si nécessaire
        }
    }

    return false; // Si aucune valeur ne fonctionne, retour en arrière
}

private (int, int)? FindEmptyCell()
{
    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            if (Grid.Cells[row, col] == 0)
            {
                return (row, col);
            }
        }
    }
    return null; // Aucune cellule vide trouvée
}

private bool IsValidAssignment(int row, int col, int value)
{
    return !Grid.GetColomn(col).Contains(value) &&
           !Grid.Cells[row, :].Contains(value) &&
           !Grid.GetBoxNeighbours(row, col).Contains(value);
}

}

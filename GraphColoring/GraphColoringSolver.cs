using Sudoku.Shared;

namespace GraphColoring
{
    // Implémentation d'un solveur de Sudoku utilisant la coloration de graphes.
    public class graphcoloringsolver : ISudokuSolver
    {
        // Graph utilisé pour la coloration et une matrice pour mapper les indices de la grille de Sudoku au graph.
        private SudokuConnections sudokuGraph;
        private int[][] mappedGrid;

        // Constructeur initialise le graph et la matrice de mappage.
        public graphcoloringsolver()
        {
            sudokuGraph = new SudokuConnections();
            mappedGrid = GetMappedMatrix();
        }

        // Méthode principale pour résoudre le Sudoku.
        public SudokuGrid Solve(SudokuGrid s)
        {
            var board = new int[9, 9];  // Tableau temporaire pour copier l'état actuel du Sudoku.
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    board[i, j] = s.Cells[i, j];  // Copie chaque valeur de la grille initiale.

            var (color, given) = GraphColoringInitializeColor(board);  // Initialisation des couleurs et des valeurs données.
            if (GraphColorUtility(9, color, 1, given))  // Essaie de colorer le graphe.
            {
                int count = 1;
                for (int row = 0; row < 9; row++)  // Applique les couleurs résolues à la grille de Sudoku.
                {
                    for (int col = 0; col < 9; col++)
                    {
                        s.Cells[row, col] = color[count];  // Met à jour les cellules avec les valeurs résolues.
                        count++;
                    }
                }
            }

            return s;  // Retourne la grille résolue.
        }

        // Initialise le tableau de couleurs et détermine quelles cellules sont pré-remplies.
        private (int[] color, HashSet<int> given) GraphColoringInitializeColor(int[,] board)
        {
            var color = new int[sudokuGraph.Graph.TotalV + 1];  // Tableau pour stocker les couleurs.
            var given = new HashSet<int>();  // Ensemble pour stocker les indices des valeurs données.

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (board[row, col] != 0)
                    {
                        int idx = mappedGrid[row][col];  // Obtient l'indice du graph correspondant.
                        color[idx] = board[row, col];  // Initialise la couleur avec la valeur donnée.
                        given.Add(idx);  // Marque comme donnée.
                    }
                }
            }

            return (color, given);  // Retourne les couleurs initiales et les cellules pré-remplies.
        }

        // Fonction récursive pour essayer de colorer le graphe.
        private bool GraphColorUtility(int m, int[] color, int v, HashSet<int> given)
        {
            if (v == sudokuGraph.Graph.TotalV + 1)  // Si toutes les cellules sont colorées, retourne vrai.
                return true;

            for (int c = 1; c <= m; c++)  // Essaye chaque couleur possible.
            {
                if (IsSafe2Color(v, color, c, given))  // Vérifie si c'est sûr de colorer avec c.
                {
                    color[v] = c;  // Colore la cellule.
                    if (GraphColorUtility(m, color, v + 1, given))  // Appel récursif pour la prochaine cellule.
                        return true;
                    if (!given.Contains(v))
                        color[v] = 0;  // Retour arrière si la couleur ne convient pas.
                }
            }

            return false;  // Retourne faux si aucune couleur n'est possible.
        }

        // Vérifie si il est sûr de colorer la cellule avec une couleur donnée.
        private bool IsSafe2Color(int v, int[] color, int c, HashSet<int> given)
        {
            if (given.Contains(v) && color[v] == c)
                return true;
            else if (given.Contains(v))
                return false;

            for (int i = 1; i <= sudokuGraph.Graph.TotalV; i++)
            {
                if (color[i] == c && sudokuGraph.Graph.IsNeighbour(v, i))  // Vérifie les conflits avec les voisins.
                    return false;
            }
            return true;
        }

        // Crée la matrice de mappage des indices de Sudoku à ceux du graph.
        private int[][] GetMappedMatrix()
        {
            var matrix = new int[9][];
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int count = 1;
            for (int rows = 0; rows < 9; rows++)
            {
                for (int cols = 0; cols < 9; cols++)
                {
                    matrix[rows][cols] = count++;  // Attribue un indice unique à chaque cellule.
                }
            }
            return matrix;
        }
    }
}

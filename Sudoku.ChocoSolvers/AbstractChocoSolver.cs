using Sudoku.Shared;
using org.chocosolver.solver;
using org.chocosolver.solver.constraints;
using org.chocosolver.solver.variables;
using System;

namespace Sudoku.ChocoSolvers
{
    public abstract class AbstractChocoSolver : ISudokuSolver
    {
        protected const int GridSize = 9;    // Taille standard du Sudoku
        protected const int BlockSize = 3;   // Taille d'un sous-grille (3x3 pour le Sudoku standard)
        protected IntVar[] FlatCells { get; set; } = Array.Empty<IntVar>();

        public virtual SudokuGrid Solve(SudokuGrid grid)
        {
            // Valider la grille d'entrée
            ValidateInput(grid);

            var model = new Model("Solveur de Sudoku - LNS");

            // 1. Créer des variables pour chaque cellule avec des contraintes initiales
            var cellVariables = CreateCellVariables(model, grid);

            // Aplatir la matrice pour une utilisation avancée
            FlatCells = Flatten(cellVariables);

            // 2. Appliquer les contraintes (lignes, colonnes et blocs)
            ApplyConstraints(model, cellVariables);

            // 3. Configurer le solveur Choco avec des stratégies de recherche avancées
            var solver = GetSolver(model, cellVariables);

            // Résoudre le Sudoku
            if (solver.solve())
            {
                return ExtractSolution(grid, cellVariables);
            }
            else
            {
                throw new Exception("Aucune solution trouvée.");
            }
        }

        /// <summary>
        /// Valide la grille de Sudoku en vérifiant les doublons dans les lignes, colonnes et blocs.
        /// </summary>
        protected virtual void ValidateInput(SudokuGrid grid)
        {
            for (int i = 0; i < GridSize; i++)
            {
                if (HasDuplicatesInRow(grid.Cells, i) || HasDuplicatesInColumn(grid.Cells, i))
                {
                    throw new ArgumentException($"Grille invalide : valeurs dupliquées dans la ligne ou la colonne {i + 1}");
                }
            }
            for (int br = 0; br < BlockSize; br++)
            {
                for (int bc = 0; bc < BlockSize; bc++)
                {
                    if (HasDuplicatesInBlock(grid.Cells, br, bc))
                    {
                        throw new ArgumentException($"Grille invalide : valeurs dupliquées dans le bloc ({br + 1}, {bc + 1})");
                    }
                }
            }
        }

        private bool HasDuplicatesInRow(int[,] cells, int row)
        {
            bool[] seen = new bool[GridSize + 1];
            for (int col = 0; col < GridSize; col++)
            {
                int val = cells[row, col];
                if (val != 0)
                {
                    if (seen[val]) return true;
                    seen[val] = true;
                }
            }
            return false;
        }

        private bool HasDuplicatesInColumn(int[,] cells, int col)
        {
            bool[] seen = new bool[GridSize + 1];
            for (int row = 0; row < GridSize; row++)
            {
                int val = cells[row, col];
                if (val != 0)
                {
                    if (seen[val]) return true;
                    seen[val] = true;
                }
            }
            return false;
        }

        private bool HasDuplicatesInBlock(int[,] cells, int blockRow, int blockCol)
        {
            bool[] seen = new bool[GridSize + 1];
            int startRow = blockRow * BlockSize;
            int startCol = blockCol * BlockSize;
            for (int i = 0; i < BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    int val = cells[startRow + i, startCol + j];
                    if (val != 0)
                    {
                        if (seen[val]) return true;
                        seen[val] = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Crée des variables Choco pour chaque cellule du Sudoku avec des contraintes initiales.
        /// </summary>
        protected virtual IntVar[][] CreateCellVariables(Model model, SudokuGrid grid)
        {
            var variables = new IntVar[GridSize][];

            for (int row = 0; row < GridSize; row++)
            {
                variables[row] = new IntVar[GridSize];
                for (int col = 0; col < GridSize; col++)
                {
                    int val = grid.Cells[row, col];
                    if (val != 0)
                    {
                        // La variable est fixée à la valeur initiale
                        variables[row][col] = model.intVar($"cell_{row}_{col}", val);
                    }
                    else
                    {
                        // La variable peut prendre n'importe quelle valeur entre 1 et GridSize
                        variables[row][col] = model.intVar($"cell_{row}_{col}", 1, GridSize, false);
                    }
                }
            }

            return variables;
        }

        /// <summary>
        /// Applique l'ensemble des contraintes du Sudoku au modèle Choco.
        /// </summary>
        protected virtual void ApplyConstraints(Model model, IntVar[][] cellVariables)
        {
            // Contraintes sur les lignes et les colonnes
            for (int i = 0; i < GridSize; i++)
            {
                // Contraintes sur la ligne
                model.allDifferent(cellVariables[i]).post();
                // Contraintes sur la colonne
                model.allDifferent(GetColumn(cellVariables, i)).post();
            }

            // Contraintes sur les blocs (sous-grilles)
            for (int blockRow = 0; blockRow < BlockSize; blockRow++)
            {
                for (int blockCol = 0; blockCol < BlockSize; blockCol++)
                {
                    model.allDifferent(GetBlock(cellVariables, blockRow, blockCol)).post();
                }
            }
        }

        /// <summary>
        /// Configure le solveur Choco avec des stratégies de recherche avancées.
        /// </summary>
        public virtual Solver GetSolver(Model model, IntVar[][] cellVariables)
        {
            var solver = model.getSolver();
            // Configurer des stratégies avancées ici si nécessaire (peut être redéfini par les sous-classes)
            return solver;
        }

        /// <summary>
        /// Extrait la solution du solveur Choco et remplit la grille de Sudoku.
        /// </summary>
        protected virtual SudokuGrid ExtractSolution(SudokuGrid grid, IntVar[][] cells)
        {
            for (int row = 0; row < GridSize; row++)
            {
                for (int col = 0; col < GridSize; col++)
                {
                    grid.Cells[row, col] = cells[row][col].getValue();
                }
            }
            return grid;
        }

        /// <summary>
        /// Aplati une matrice 2D de IntVar en un tableau 1D.
        /// </summary>
        protected static IntVar[] Flatten(IntVar[][] matrix)
        {
            IntVar[] flat = new IntVar[GridSize * GridSize];
            int index = 0;
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    flat[index++] = matrix[i][j];
                }
            }
            return flat;
        }

        /// <summary>
        /// Récupère une colonne de la matrice de variables.
        /// </summary>
        private IntVar[] GetColumn(IntVar[][] grid, int col)
        {
            IntVar[] column = new IntVar[GridSize];
            for (int row = 0; row < GridSize; row++)
            {
                column[row] = grid[row][col];
            }
            return column;
        }

        /// <summary>
        /// Récupère un bloc (sous-grille 3x3) de la matrice de variables.
        /// </summary>
        private IntVar[] GetBlock(IntVar[][] grid, int blockRow, int blockCol)
        {
            IntVar[] block = new IntVar[BlockSize * BlockSize];
            int index = 0;
            int startRow = blockRow * BlockSize;
            int startCol = blockCol * BlockSize;
            for (int i = 0; i < BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    block[index++] = grid[startRow + i][startCol + j];
                }
            }
            return block;
        }
    }
}

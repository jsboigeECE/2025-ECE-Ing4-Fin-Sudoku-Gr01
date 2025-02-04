using System;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    /// <summary>
    /// Solveur optimisé qui représente chaque cellule du sudoku par un vecteur de bits (BitVecExpr).
    /// La largeur est fixée à 4 bits (permettant de représenter des valeurs de 0 à 15),
    /// et on contraint chaque cellule à prendre une valeur entre 1 et 9.
    /// Les contraintes d'unicité sur les lignes, colonnes et blocs 3x3 sont ensuite appliquées.
    /// </summary>
    public class Z3BitVecSolver : ISudokuSolver
    {
        // Largeur en bits pour représenter une cellule (4 bits suffisent pour les valeurs 1..9)
        private const uint BitWidth = 4;

        public SudokuGrid Solve(SudokuGrid s)
        {
            using (var context = new Context())
            {
                Solver solver = context.MkSolver();

                // Création d'une matrice 9x9 de variables de type BitVecExpr
                BitVecExpr[,] cells = new BitVecExpr[9, 9];
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        // Chaque cellule est déclarée comme une constante de type BitVec de largeur BitWidth
                        cells[i, j] = (BitVecExpr)context.MkConst($"cell_{i}_{j}", context.MkBitVecSort(BitWidth));
                        
                        // Contraintes de domaine : 1 <= cellule <= 9
                        solver.Assert(context.MkBVUGE(cells[i, j], context.MkBV(1, BitWidth)));
                        solver.Assert(context.MkBVULE(cells[i, j], context.MkBV(9, BitWidth)));
                    }
                }

                // Contrainte pour les cellules déjà fixées dans la grille initiale
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (s.Cells[i, j] != 0)
                        {
                            solver.Assert(context.MkEq(cells[i, j], context.MkBV(s.Cells[i, j], BitWidth)));
                        }
                    }
                }

                // Contrainte d'unicité sur les lignes
                for (int i = 0; i < 9; i++)
                {
                    Expr[] row = new Expr[9];
                    for (int j = 0; j < 9; j++)
                    {
                        row[j] = cells[i, j];
                    }
                    solver.Assert(context.MkDistinct(row));
                }

                // Contrainte d'unicité sur les colonnes
                for (int j = 0; j < 9; j++)
                {
                    Expr[] col = new Expr[9];
                    for (int i = 0; i < 9; i++)
                    {
                        col[i] = cells[i, j];
                    }
                    solver.Assert(context.MkDistinct(col));
                }

                // Contrainte d'unicité sur les blocs 3x3
                for (int blockRow = 0; blockRow < 3; blockRow++)
                {
                    for (int blockCol = 0; blockCol < 3; blockCol++)
                    {
                        Expr[] block = new Expr[9];
                        int idx = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                block[idx++] = cells[blockRow * 3 + i, blockCol * 3 + j];
                            }
                        }
                        solver.Assert(context.MkDistinct(block));
                    }
                }

                // Vérification et extraction du modèle solution (si le problème est satisfaisable)
                if (solver.Check() == Status.SATISFIABLE)
                {
                    Model model = solver.Model;
                    SudokuGrid solvedGrid = new SudokuGrid();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            // Conversion de la valeur du BitVec en entier
                            solvedGrid.Cells[i, j] = ((BitVecNum)model.Evaluate(cells[i, j])).Int;
                        }
                    }
                    return solvedGrid;
                }
                else
                {
                    throw new Exception("Aucune solution trouvée.");
                }
            }
        }
    }
}

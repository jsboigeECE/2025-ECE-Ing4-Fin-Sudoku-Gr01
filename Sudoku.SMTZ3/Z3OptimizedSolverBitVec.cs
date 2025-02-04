using System;
using System.Collections.Generic;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    /// <summary>
    /// Solveur optimisé pour le Sudoku utilisant Z3.
    /// Il représente chaque cellule par un vecteur de bits (BitVecExpr) de 4 bits,
    /// applique une configuration personnalisée pour le contexte Z3,
    /// ajoute les contraintes classiques (domaine, lignes, colonnes, blocs 3x3)
    /// et utilise un Goal avec une tactique de simplification.
    /// </summary>
    public class Z3OptimizedSolverBitVec : ISudokuSolver
    {
        // Largeur en bits pour représenter une cellule (4 bits suffisent pour 1 à 9)
        private const uint BitWidth = 4;

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Configuration personnalisée du contexte Z3 pour optimiser la résolution
            var cfg = new Dictionary<string, string>
            {
                { "auto_config", "false" },          // Désactivation de l’auto configuration
                { "timeout", "5000" },               // Timeout fixé à 5000 ms (5 s)
                { "smt.phase_selection", "5" },      // Ajustement de la sélection de phase
                { "smt.case_split", "4" }            // Découpage de cas pour améliorer la stratégie
                //  "smt.relevancy" si besoin
            };

            using (var context = new Context(cfg))
            {
                Solver solver = context.MkSolver();
                BitVecExpr[,] cells = new BitVecExpr[9, 9];

                // Création de la matrice de cellules en BitVecExpr avec contraintes de domaine
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        cells[i, j] = (BitVecExpr)context.MkConst($"cell_{i}_{j}", context.MkBitVecSort(BitWidth));
                        // Contrainte sur le domaine : 1 <= cell <= 9
                        solver.Assert(context.MkBVUGE(cells[i, j], context.MkBV(1, BitWidth)));
                        solver.Assert(context.MkBVULE(cells[i, j], context.MkBV(9, BitWidth)));
                    }
                }

                // Contraintes pour les cellules déjà fixées dans la grille initiale
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

                // Création d'un Goal et ajout de toutes les assertions du solveur
                Goal goal = context.MkGoal(true, false, false);
                goal.Add(solver.Assertions);

                // Application de la tactique de simplification sur le Goal
                Tactic tactic = context.MkTactic("simplify");
                ApplyResult ar = tactic.Apply(goal);

                // Réinitialisation du solveur et ajout du résultat simplifié
                solver.Reset();
                // On ajoute le résultat simplifié du premier sous-objectif
                solver.Assert(ar.Subgoals[0].AsBoolExpr());

                // Vérification de la satisfaisabilité et extraction du modèle solution
                if (solver.Check() == Status.SATISFIABLE)
                {
                    Model model = solver.Model;
                    SudokuGrid solvedGrid = new SudokuGrid();
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
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

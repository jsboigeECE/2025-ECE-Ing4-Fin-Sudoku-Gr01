using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.SMTZ3
{
    public class Z3SubstitutionSolver : Z3SolverBase
    {
        // Liste de paires (variable, constante) pour la substitution
        private List<Tuple<IntExpr, IntNum>> substitutions;

        /// <summary>
        /// Au lieu d’assertion, on enregistre les substitutions à effectuer.
        /// </summary>
        protected override void AddInitialConstraints(SudokuGrid s, Context context, Solver solver, IntExpr[,] cells)
        {
            substitutions = new List<Tuple<IntExpr, IntNum>>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        // Enregistrer la substitution : la variable est remplacée par la constante correspondante
                        substitutions.Add(new Tuple<IntExpr, IntNum>(cells[i, j], context.MkInt(s.Cells[i, j])));
                    }
                }
            }
            // Remarquez qu'ici, aucune contrainte "cell == valeur" n'est ajoutée.
        }

        /// <summary>
        /// Ajoute les contraintes sur les lignes, colonnes et blocs en appliquant les substitutions
        /// pour remplacer directement les variables fixées par leurs constantes.
        /// </summary>
        protected override void AddGenericConstraints(Context context, Solver solver, IntExpr[,] cells)
        {
            // Préparation des tableaux de substitution
            var from = substitutions.Select(t => (Expr)t.Item1).ToArray();
            var to = substitutions.Select(t => (Expr)t.Item2).ToArray();

            // Contrainte sur les lignes avec substitution
            for (int i = 0; i < 9; i++)
            {
                var rowCells = new Expr[9];
                for (int j = 0; j < 9; j++)
                {
                    // Substituer les cellules fixes
                    rowCells[j] = cells[i, j].Substitute(from, to);
                }
                solver.Assert(context.MkDistinct(rowCells));
            }

            // Contrainte sur les colonnes avec substitution
            for (int j = 0; j < 9; j++)
            {
                var colCells = new Expr[9];
                for (int i = 0; i < 9; i++)
                {
                    colCells[i] = cells[i, j].Substitute(from, to);
                }
                solver.Assert(context.MkDistinct(colCells));
            }

            // Contrainte sur les blocs 3x3 avec substitution
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    var blockCells = new Expr[9];
                    int idx = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            blockCells[idx++] = cells[blockRow * 3 + i, blockCol * 3 + j].Substitute(from, to);
                        }
                    }
                    solver.Assert(context.MkDistinct(blockCells));
                }
            }
        }
    }
}

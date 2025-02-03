using Sudoku.Shared;
using Microsoft.Z3;
using System.Linq;

namespace Sudoku.LinqToZ3Solvers;                                                                                                                                                                           // Namespace Sudoku.LinqToZ3Solvers : Contient la classe de résolution utilisant Z3

public class PermuteLinqToZ3 : ISudokuSolver                                                                                                                                                                // Classe PermuteLinqToZ3 : Implémente l'interface ISudokuSolver pour résoudre un Sudoku 
{
    public SudokuGrid Solve(SudokuGrid s)                                                                                                                                                                   // Méthode Solve(SudokuGrid s) : Prend une grille de Sudoku en entrée et retourne la grille résolue
    {
        using Context ctx = new Context();
        Solver solver = ctx.MkSolver();

        // DÉFINITION DES VARIABLES SMT : UNE MATRICE 9X9 DE VARIABLES ENTIÈRES                                                                                                                             // Création d'un contexte SMT pour gérer les contraintes (Z3)

        IntExpr[,] cells = new IntExpr[9, 9];

        // CRÉATION DES VARIABLES : CHAQUE CASE CONTIENT UNE VALEUR ENTRE 1 ET 9                                                                                                                            // Initialisation du solveur SMT
                                                                                                                                                                                                            // Déclaration de la matrice 9x9 représentant chaque case du Sudoku (type IntExpr)
        Enumerable.Range(0, 9)
                  .ToList()                                                                                                                                                                                 // Création des variables SMT : chaque case prend une valeur entre 1 et 9                                                                                                                  // Déclaration de la matrice 9x9 représentant chaque case du Sudoku (type IntExpr)
                  .ForEach(r => Enumerable.Range(0, 9)
                      .ToList()
                      .ForEach(c => cells[r, c] = (IntExpr)ctx.MkIntConst($"cell_{r}_{c}")));

        // CONTRAINTE 1 : CHAQUE CASE CONTIENT UNE VALEUR ENTRE 1 ET 9
        solver.Add(ctx.MkAnd(
            cells.Cast<IntExpr>()
                 .Select(cell => ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), cell), ctx.MkLe(cell, ctx.MkInt(9))))                                                                                                     // Contrainte 1 : Chaque case doit contenir une valeur entre 1 et 9
                 .ToArray()
        ));

        // CONTRAINTES POUR CHAQUE LIGNE
        solver.Add(
            Enumerable.Range(0, 9)                                                                                                                                                                         // Contrainte 2 : Chaque ligne doit être une permutation unique des chiffres de 1 à 9

                      .Select(r => ctx.MkDistinct(Enumerable.Range(0, 9).Select(c => cells[r, c]).ToArray()))                                                                                              // Contrainte 3 : Chaque colonne doit être une permutation unique des chiffres de 1 à 9
        );

        // CONTRAINTES POUR CHAQUE COLONNE
        solver.Add(
            Enumerable.Range(0, 9)
                      .Select(c => ctx.MkDistinct(Enumerable.Range(0, 9).Select(r => cells[r, c]).ToArray()))                                                                                              
        );

        // CONTRAINTE 4 : CHAQUE BLOC 3×3 CONTIENT LES NOMBRES DE 1 À 9
        solver.Add(
            from br in Enumerable.Range(0, 3)                                                                                                                                                               // Contrainte 4 : Chaque bloc 3x3 doit contenir une permutation unique des chiffres de 1 à 9

            from bc in Enumerable.Range(0, 3)
            select ctx.MkDistinct(
                (from r in Enumerable.Range(0, 3)
                 from c in Enumerable.Range(0, 3)
                 select cells[br * 3 + r, bc * 3 + c]).ToArray()
            )
        );

        // CONTRAINTE 5 : INTÉGRATION DES VALEURS CONNUES                                                                                                                                                  // Contrainte 5 : Respect des valeurs initiales présentes dans la grille de Sudoku                                                                                                                                                   
        solver.Add(
            from r in Enumerable.Range(0, 9)
            from c in Enumerable.Range(0, 9)
            where s.Cells[r, c] != 0                                                                                                                                                                       
            select ctx.MkEq(cells[r, c], ctx.MkInt(s.Cells[r, c]))
        );

        // RÉSOLUTION DU SUDOKU
        if (solver.Check() == Status.SATISFIABLE)                                                                                                                                                          // Vérification de la satisfiabilité des contraintes via solver.Check() 
        {
            Model model = solver.Model;
            Enumerable.Range(0, 9)                                                                                                                                                                         // Si une solution existe, récupération des valeurs du modèle Z3
                      .ToList()
                      .ForEach(r => Enumerable.Range(0, 9)
                          .ToList()
                          .ForEach(c => s.Cells[r, c] = ((IntNum)model.Evaluate(cells[r, c])).Int));                                                                                                       // Mise à jour de la grille Sudoku avec les valeurs trouvées
        }

        return s;                                                                                                                                                                                          // Retourne la grille résolue ou inchangée si aucune solution n'existe
    }
}

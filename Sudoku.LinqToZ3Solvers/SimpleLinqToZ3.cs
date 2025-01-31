using Sudoku.Shared;                                                                                                                                                                        // Sudoku.Shared : Contient la classe SudokuGrid qui permet de représenter la grille de Sudoku
using Microsoft.Z3;                                                                                                                                                                         // Microsoft.Z3 : Bibliothèque qui permet d'utiliser le solver SMT (Satisfiability Modulo Theories) Z3


namespace Sudoku.LinqToZ3Solvers;

public class SimpleLinqToZ3 : ISudokuSolver                                                                                                                                                    // ISudokuSolver : Interface qui garantit que le solveur SMTLinqToZ3 implémente la méthode Solve(SudokuGrid s)
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        // CREATION DU CONTEXTE Z3
        using Context ctx = new Context();                                                                                                                                                  // Context : Environnement dans lequel on va définir les variables et les contraintes SMT 
        Solver solver = ctx.MkSolver();                                                                                                                                                     // MkSolver() : Crée une instance du soleur qui va manipuler ces contraintes. 

        // CREATION DES VARIABLES SMT POUR CHAQUE CELLULE
        IntExpr[] cells = Enumerable.Range(0, 81) // Enumerable.Range génère les indices                                                                                                    // Enumerable.Range(0,81) : Génère une séquence d'indices de 0 à 80 (81 cases du Sudoku)
                                    .Select(i => (IntExpr)ctx.MkIntConst($"cell_{i}")) // Select crée les variables SMT                                                                     // ctx.MkConst($"cell_{i}") : Crée une variable SMT entière pour chaque case
                                    .ToArray();                                                                                                                                             // ToArray() : Convertit le résultat en un tableau de 81 variables SMT

        // AJOUT DES CONTRAINTES : LES VALEURS DOIVENT ÊTRE ENTRE 1 ET 9
        solver.Add(cells.Select(cell => ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), cell), ctx.MkLe(cell, ctx.MkInt(9))))); // Select applique la contrainte MkAnd(...) à chaque cellule               // ctx.MkLe(ctx.MkInt(1), cell) : cell >= 1    &   ctx.MkLe(cell, ctx.MkInt(9)) : cell <= 9 


        // CONTRAINTES POUR LES LIGNES
        solver.Add(Enumerable.Range(0, 9)                                                                                                                                                    
                             .Select(r => ctx.MkDistinct(cells.Skip(r * 9).Take(9).ToArray()))); // Select crée une contrainte MkDistinct pour chaque ligne                                 // cell.Skip(r*9).Take(9) : Récupère 9 céllules successives (une ligne)     &     ctx.MkDistinct(...) : Impose que toutes les valeurs soient différentes dans cette ligne     &     Select(r => ...) : Applique cette contrainte aux 9 lignes du Sudoku                         

        // CONTRAINTES POUR LES COLONNES
        solver.Add(Enumerable.Range(0, 9)
                             .Select(c => ctx.MkDistinct(Enumerable.Range(0, 9).Select(r => cells[r * 9 + c]).ToArray()))); // Select crée une contrainte MkDistinct pour chaque colonne    // Enumerable.Range(0,9).Select(r => cells[r * 9 + c]) : Récupère toutes les cellules de la colonne c     &     ctx.MkDistinct(...) : Impose que toutes ces valeurs soient différentes     &     Select(c => ...) : Applique cette contrainte aux 9 colonnes 

        // CONTRAINTES POUR LES BLOCS 3X3
        solver.Add(
            from br in Enumerable.Range(0, 3) // Utilisatioin de from ... in pour générer les indices                                                                                       // br parcourt les 3 blocs horizontaux
            from bc in Enumerable.Range(0, 3)                                                                                                                                               // bc parcourt les 3 blocs verticaux
            select ctx.MkDistinct( // Select pour construire chaque bloc                                                                                                                    // ctx.MkDistinct(...) : Implique que toutes les valeurs du bloc soient différentes
                 (from r in Enumerable.Range(0, 3)                                                                                                                                          // r parcourt les 3 lignes à l'intérieur d'un bloc
                  from c in Enumerable.Range(0, 3)                                                                                                                                          // c parcourt les 3 colonnes à l'intérieur d'un bloc
                  select cells[(br * 3 + r) * 9 + (bc * 3 + c)]).ToArray()                                                                                                                  // cells[(br * 3 + r) * 9 + (bc * 3 + c)] : Récupère chaque case du bloc
            )
        );

        // AJOUT DES CONTRAINTES DES CELLULES DEJA REMPLIES
        solver.Add(
            Enumerable.Range(0, 81)
                      .Where(i => s.Cells[i / 9, i % 9] != 0) // Where filtre les cellules déjà remplies                                                                                    // Where(i => s.Cells[i / 9, i % 9] != 0) : Filtre les cases déjà remplies
                      .Select(i => ctx.MkEq(cells[i], ctx.MkInt(s.Cells[i / 9, i % 9]))) // Select applique MkEq                                                                            // ctx.MkEq(cells[i], ctx.MkInt(s.Cells[i / 9, i % 9])) : Fixe la valeur SMT à la valeur connue
        );

        // RESOLUTION DU SUDOKU
        if (solver.Check() == Status.SATISFIABLE)                                                                                                                                           // solver.Check() == Status.SATISFIABLE : Vérifie si une solution existe
        {
            Model model = solver.Model;                                                                                                                                                     // solver.Model : Récupère le modèle SMT trouvé
            Enumerable.Range(0, 81)
                      .ToList() // ToList().ForEach(...) applique directement l'évaluation du modèle. 
                      .ForEach(i => s.Cells[i / 9, i % 9] = ((IntNum)model.Evaluate(cells[i])).Int);                                                                                        // model.Evaluate(cells[i]) : Récupère la valeur de la cellule SMT     &     ToList().ForEach(...) : Met à jour la grille Sudoku avec la solution trouvée
        }
        return s;
    }
}


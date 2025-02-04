using Sudoku.Shared;                                                                                                                                // Sudoku.Shared : Contient la classe SudokuGrid qui permet de représenter la grille de Sudoku
using Microsoft.Z3;                                                                                                                                    // Microsoft.Z3 : Bibliothèque qui permet d'utiliser le solver SMT (Satisfiability Modulo Theories) Z3


namespace Sudoku.LinqToZ3Solvers
{
    public class BooleenLinqToZ3 : ISudokuSolver// ISudokuSolver : Interface qui garantit que le solveur SMTLinqToZ3 implémente la méthode Solve(SudokuGrid s)
    {                                                                                                            
        public SudokuGrid Solve(SudokuGrid s)                                                                                                       //Cette méthode prend une grille Sudoku (s), la résout et retourne la grille complétée
        {
            using Context ctx = new Context();                                                                                                      // Context : Environnement dans lequel on va définir les variables et les contraintes SMT
            Solver solver = ctx.MkSolver();                                                                                                         // MkSolver() : Crée une instance du soleur qui va manipuler ces contraintes.

            // 🔹 Définition des variables : une matrice 9x9x9 de booléens
            BoolExpr[,,] cells = new BoolExpr[9, 9, 9];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    for (int v = 0; v < 9; v++)
                    {
                        cells[r, c, v] = ctx.MkBoolConst($"cell_{r}{c}{v}");                                                                        //cells[r, c, v] est un tableau 3D de booléens ; r ligne de 0 à 8 ; c colonne de 0 à 8 ; v valeur de 0 à 8
                    }
                }
            }

            //  Contrainte 1 : Chaque cellule contient exactement une valeur
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    solver.Add(ctx.MkOr(Enumerable.Range(0, 9).Select(v => cells[r, c, v]).ToArray())); // Au moins une valeur est true                //Une case doit contenir au moins un chiffre (MkOr). // Enumerable.Range(0, 9) crée une liste des variables booléennes représentant les 9 chiffres possibles pour la cellule (r, c)

                    // Interdire plusieurs valeurs en même temps
                    for (int v1 = 0; v1 < 9; v1++)
                    {
                        for (int v2 = v1 + 1; v2 < 9; v2++)
                        {
                            solver.Add(ctx.MkNot(ctx.MkAnd(cells[r, c, v1], cells[r, c, v2])));                                                     //Une case ne peut pas contenir deux chiffres en même temps (MkNot(MkAnd(...)))
                        }
                    }
                }
            }

            //  Contrainte 2 : Chaque ligne contient chaque chiffre une seule fois
            for (int r = 0; r < 9; r++)
            {
                for (int v = 0; v < 9; v++)
                {
                    for (int c1 = 0; c1 < 9; c1++)
                    {
                        for (int c2 = c1 + 1; c2 < 9; c2++)
                        {
                            solver.Add(ctx.MkNot(ctx.MkAnd(cells[r, c1, v], cells[r, c2, v])));                                                // Ajoute une contrainte interdisant que la valeur `v` apparaisse simultanément dans deux colonnes différentes `c1` et `c2` sur la même ligne `r`
                        }
                    }
                }
            }

            //  Contrainte 3 : Chaque colonne contient chaque chiffre une seule fois
            for (int c = 0; c < 9; c++)
            {
                for (int v = 0; v < 9; v++)                                                                            //  Comparaison entre chaque paire de colonnes dans la ligne
                {
                    for (int r1 = 0; r1 < 9; r1++)
                    {
                        for (int r2 = r1 + 1; r2 < 9; r2++)
                        {
                            solver.Add(ctx.MkNot(ctx.MkAnd(cells[r1, c, v], cells[r2, c, v])));                         // Si une case (r, c1) contient v, alors (r, c2) ne peut pas contenir v
                        }
                    }
                }
            }

            //  Contrainte 4 : Chaque bloc 3×3 contient chaque chiffre une seule fois
            for (int br = 0; br < 3; br++)
            {
                for (int bc = 0; bc < 3; bc++)
                {
                    for (int v = 0; v < 9; v++)
                    {
                        var positions = from r in Enumerable.Range(0, 3)                                                                  //Enumerable.Range(0, 3) génère les indices 0, 1, 2 pour parcourir les lignes et colonnes d'un bloc 3×3
                                        from c in Enumerable.Range(0, 3)                                                                  //from r in Enumerable.Range(0, 3) et from c in Enumerable.Range(0, 3) permettent d'obtenir toutes les positions des cellules d'un bloc 3×3.
                                        select cells[br * 3 + r, bc * 3 + c, v];                                                          //récupère les variables booléennes associées à ces cellules et au chiffre v

                        foreach (var (cell1, cell2) in positions.SelectMany((v1, i) => positions.Skip(i + 1), (v1, v2) => (v1, v2)))      //positions.Skip(i + 1) empêche de sélectionner deux fois la même paire. (v1, v2) => (v1, v2) forme les couples de cellules
                        {
                            solver.Add(ctx.MkNot(ctx.MkAnd(cell1, cell2)));                                                               // Si une case (r1, c) contient v, alors (r2, c) ne peut pas contenir v  ,assurer que deux cellules d'un même bloc ne peuvent pas contenir la même valeur.
                        }
                    }
                }
            }

            //  Contrainte 5 : Intégration des valeurs initiales du Sudoku
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (s.Cells[r, c] != 0)                                                                         // verifie que la cellule contient deja une valeur
                    {
                        solver.Add(cells[r, c, s.Cells[r, c] - 1]);                                                // s.Cells[r, c] - 1 : Convertit la valeur Sudoku (1-9) en index du tableau (0-8), Fixe la valeur connue pour eviter quelle change
                    }
                }
            }

            // 🧩 Résolution du Sudoku
            Status status = solver.Check();                                                                         //Vérifie si une solution existe (solver.Check()).

            if (status == Status.SATISFIABLE)                                                                        // si solution existe
            {
                Model model = solver.Model;                                                                         // Récupère le modèle (la solution trouvée par Z3)

                // Extraction des valeurs de la solution
                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        bool found = false;                                                                          // Indicateur pour vérifier si une valeur a été trouvée
                        for (int v = 0; v < 9; v++)
                        {
                            var eval = model.Evaluate(cells[r, c, v]);                                                // Évalue la cellule (r, c) pour la valeur v
                            if (eval != null && eval.IsTrue)                                                          // Si l'évaluation retourne vrai, c'est la bonne valeur
                            {
                                s.Cells[r, c] = v + 1;                                                               //Si oui, récupère les valeurs et remplit la grille s.Cells[r, c] en ajoutant 1 pour retrouver l'echelle 1-9
                                found = true;
                                break;                                                                               // On a trouvé la valeur, inutile de tester les autres
                            }
                        }
                        if (!found)                                                                                  // Si aucune valeur n'a été trouvée pour la cellule
                        {
                            Console.WriteLine($" Aucune valeur trouvée pour la cellule ({r}, {c}) !");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(" Aucune solution trouvée.");
            }

            return s;
        }
    }
}

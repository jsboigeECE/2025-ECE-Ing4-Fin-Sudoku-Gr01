using org.chocosolver.solver;
using org.chocosolver.solver.variables;
using org.chocosolver.solver.search.strategy;
using org.chocosolver.solver.search.strategy.selectors.values;
using org.chocosolver.solver.search.strategy.selectors.variables;

namespace Sudoku.ChocoSolvers
{
    /// <summary>
    /// Solveur Sudoku basé sur l'heuristique du degré et la propagation avancée des contraintes.
    /// </summary>
    public class ChocoDegreePropagationSolver : AbstractChocoSolver
    {
        /// <summary>
        /// Configure la recherche en utilisant l'heuristique du degré (Degree Heuristic)
        /// et la sélection minimale de domaine.
        /// </summary>
        /// <param name="model">Le modèle Choco.</param>
        /// <param name="cells">La matrice des variables de cellules.</param>
        protected void ConfigureSearch(Model model, IntVar[][] cells)
        {
            var solver = model.getSolver();
            solver.setSearch(
                Search.intVarSearch(
                    new DomOverWDeg(FlatCells, 0),
                    new IntDomainMin(),
                    FlatCells
                )
            );
        }

        /// <summary>
        /// Applique les contraintes de base et active la propagation des Naked Singles.
        /// </summary>
        /// <param name="model">Le modèle Choco.</param>
        /// <param name="cells">La matrice des variables de cellules.</param>
        protected override void ApplyConstraints(Model model, IntVar[][] cells)
        {
            base.ApplyConstraints(model, cells);
            ApplyNakedSinglesPropagation();
        }

        /// <summary>
        /// Parcourt toutes les cellules et instancie celles dont le domaine est réduit à une seule valeur.
        /// </summary>
        private void ApplyNakedSinglesPropagation()
        {
            foreach (var cell in FlatCells)
            {
                if (cell.getDomainSize() == 1)
                {
                    // On instancie la cellule avec sa seule valeur possible.
                    cell.instantiateTo(cell.getValue(), null);
                }
            }
        }
    }
}

using org.chocosolver.solver;
using org.chocosolver.solver.variables;
using org.chocosolver.solver.search.strategy.selectors.values;
using org.chocosolver.solver.search.strategy.selectors.variables;

namespace Sudoku.ChocoSolvers
{
    /// <summary>
    /// Solveur Sudoku optimisé utilisant une stratégie avancée de sélection des variables et des valeurs.
    /// </summary>
    public class ChocoSolverVariableSelector : AbstractChocoSolver
    {
        /// <summary>
        /// Configure la stratégie de recherche avec un ordonnancement amélioré des variables et des valeurs.
        /// </summary>
        /// <param name="model">Le modèle Choco.</param>
        /// <param name="cells">La matrice des variables de cellules.</param>
        /// <returns>Le solveur configuré.</returns>
        public override Solver GetSolver(Model model, IntVar[][] cells)
        {
            // Aplatir la matrice 2D des cellules en un tableau 1D pour appliquer la stratégie
            IntVar[] flatCells = Flatten(cells);

            // Configuration de la stratégie de recherche :
            // - Utilise l'heuristique FirstFail pour sélectionner la variable avec le plus petit domaine.
            // - Utilise IntDomainMin pour choisir la plus petite valeur disponible.
            model.getSolver().setSearch(
                org.chocosolver.solver.search.strategy.Search.intVarSearch(
                    new FirstFail(model),
                    new IntDomainMin(),
                    flatCells
                )
            );

            // Désactiver la stratégie de redémarrage pour éviter des interruptions coûteuses lors de la recherche d'une solution.
            model.getSolver().setNoGoodRecordingFromRestarts();

            return model.getSolver();
        }
    }
}

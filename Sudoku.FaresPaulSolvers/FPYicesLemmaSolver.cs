using System;

namespace Sudoku.FaresPaulSolvers;

public class YicesSolverTheoryLemma : BaseYicesSolver
{
    protected override string GetHeuristicConfig()
    {
        return 
            "(set-option :cache-tclauses true)\n" +    // Active la mise en cache des clauses de théorie
            "(set-option :tclause-size 10)\n" +       // Définit une taille limite pour les clauses
            "(set-option :dyn-ack true)\n" +          // Active les lemmes d'Ackermann dynamiques
            "(set-option :max-ack 50)\n" +            // Limite le nombre de lemmes d'Ackermann
            "(set-option :dyn-ack-threshold 5)\n" +   // Seuil pour générer plus ou moins de lemmes
            "(set-option :dyn-bool-ack true)\n" +     // Active les lemmes d'Ackermann pour les termes booléens
            "(set-option :max-bool-ack 30)\n" +       // Limite le nombre de lemmes booléens
            "(set-option :dyn-bool-ack-threshold 3)\n";// Ajuste l'agressivité des lemmes booléens
    }
}

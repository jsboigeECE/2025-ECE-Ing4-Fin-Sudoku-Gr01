using System;
using System.Collections.Generic;
using Sudoku.Shared;

namespace Sudoku.recuit_simule
{
    public class Recuit_V2 : ISudokuSolver
    {
        private static readonly Random rand = new Random();

        public SudokuGrid Solve(SudokuGrid sudokuToSolve)
        {
            // 1) Convertir la grille SudokuGrid en tableau [9,9]
            int[,] grilleBase = SudokuGridToArray(sudokuToSolve);

            // 2) Paramètres du recuit simulé
            double temperatureInitiale = 10.0;
            double alpha = 0.995;
            int nbIterations = 200000000;
            
            // 3) Lancer l'algo de recuit simulé
            //    On fixe un seuil de stagnation (exemple : 3000)
            var (solutionTrouvee, coutFinal, iterationsEffectuees) = RecuitSimuleSudoku(
                grilleBase,
                temperatureInitiale,
                alpha,
                nbIterations,
                seuilStagnation: 30000
            );

            // 4) Reconstruire un SudokuGrid depuis la solution
            SudokuGrid solvedGrid = ArrayToSudokuGrid(solutionTrouvee);

            // Afficher le coût final et le nombre d'itérations
            Console.WriteLine($"Recuit terminé après {iterationsEffectuees} itérations. Coût final : {coutFinal}");
            
            return solvedGrid;
        }

        /// <summary>
        /// Algorithme principal de recuit simulé pour Sudoku
        /// avec mécanisme de "restart" en cas de stagnation,
        /// mais uniquement si la température est inférieure à un seuil minimal.
        /// </summary>
        private (int[,], int, int) RecuitSimuleSudoku(
            int[,] grilleBase,
            double tempInitiale,
            double alpha,
            int nbIterations,
            int seuilStagnation
        )
        {
            // Générer une solution initiale
            int[,] solutionCourante = GenererSolutionInitiale(grilleBase);
            int coutCourant = CalculerCout(solutionCourante);

            // Meilleur état rencontré
            int[,] meilleureSolution = CopierGrille(solutionCourante);
            int meilleurCout = coutCourant;

            double T = tempInitiale;

            // Pour détecter la stagnation
            int iterationsSansAmelioration = 0;

            // Variable pour suivre l'index d'itération
            int iteration = 0;

            // Définition d'un seuil "température minimale"
            double temperatureMin = 0.0000001;

            for (iteration = 0; iteration < nbIterations; iteration++)
            {
                // Vérification immédiate : si on a déjà coût 0, on s'arrête
                if (coutCourant == 0)
                {
                    Console.WriteLine($"Sudoku résolu à l'itération {iteration} !");
                    break;
                }

                // Générer un voisin
                int[,] solutionVoisin = ChoisirVoisin(solutionCourante, grilleBase);
                int coutVoisin = CalculerCout(solutionVoisin);

                int delta = coutVoisin - coutCourant;

                if (delta < 0)
                {
                    // Amélioration
                    solutionCourante = solutionVoisin;
                    coutCourant = coutVoisin;
                }
                else
                {
                    // Dégradation : acceptation avec probabilité exp(-delta / T)
                    double prob = Math.Exp(-delta / T);
                    if (rand.NextDouble() < prob)
                    {
                        solutionCourante = solutionVoisin;
                        coutCourant = coutVoisin;
                    }
                }

                // Mise à jour du meilleur état connu
                if (coutCourant < meilleurCout)
                {
                    meilleurCout = coutCourant;
                    meilleureSolution = CopierGrille(solutionCourante);
                    iterationsSansAmelioration = 0; // on réinitialise la stagnation
                }
                else
                {
                    // Sinon, on incrémente le nombre d'itérations sans amélioration
                    iterationsSansAmelioration++;
                }

                // On vérifie la stagnation
                if (iterationsSansAmelioration > seuilStagnation)
                {
                    // --> On NE redémarre QUE si T est "minimale"
                    if (T <= temperatureMin)
                    {
                        Console.WriteLine("=== Redémarrage du recuit  ===");
                        solutionCourante = GenererSolutionInitiale(grilleBase);
                        coutCourant = CalculerCout(solutionCourante);
                        iterationsSansAmelioration = 0;
                    }
                    
                }

                // Diminution de la température
                T *= alpha;
            }

            // On sort de la boucle : iteration est soit < nbIterations (si on a break),
            // soit = nbIterations si on n'a pas résolu à temps.
            int iterationsEffectuees = iteration; 
            return (meilleureSolution, meilleurCout, iterationsEffectuees);
        }

        /// <summary>
        /// Génère une solution initiale plus "propre"
        /// en remplissant chaque ligne avec les chiffres manquants.
        /// </summary>
        private int[,] GenererSolutionInitiale(int[,] grilleBase)
        {
            int[,] solution = CopierGrille(grilleBase);

            for (int row = 0; row < 9; row++)
            {
                // Récupère les chiffres déjà présents dans la ligne
                HashSet<int> used = new HashSet<int>();
                for (int col = 0; col < 9; col++)
                {
                    if (solution[row, col] != 0)
                    {
                        used.Add(solution[row, col]);
                    }
                }

                // Liste des chiffres manquants
                List<int> missing = new List<int>();
                for (int val = 1; val <= 9; val++)
                {
                    if (!used.Contains(val))
                    {
                        missing.Add(val);
                    }
                }

                // On mélange (shuffle) les chiffres manquants
                Shuffle(missing);

                // On remplit les cases vides de la ligne avec les chiffres manquants
                int indexMissing = 0;
                for (int col = 0; col < 9; col++)
                {
                    if (solution[row, col] == 0)
                    {
                        solution[row, col] = missing[indexMissing];
                        indexMissing++;
                    }
                }
            }

            return solution;
        }

        /// <summary>
        /// Génère un voisin en échangeant deux cases libres 
        /// dans la même ligne OU dans le même bloc 3x3.
        /// </summary>
        private int[,] ChoisirVoisin(int[,] solutionCourante, int[,] grilleBase)
        {
            int[,] voisin = CopierGrille(solutionCourante);

            bool echangeEffectue = false;
            int tentative = 0;

            while (!echangeEffectue && tentative < 50)
            {
                // Décider aléatoirement de faire un swap dans une ligne ou un bloc
                bool swapParLigne = (rand.Next(2) == 0);

                if (swapParLigne)
                {
                    // Choisir une ligne au hasard
                    int row = rand.Next(0, 9);

                    // Récupérer les positions libres de cette ligne
                    List<int> freeCols = new List<int>();
                    for (int c = 0; c < 9; c++)
                    {
                        if (grilleBase[row, c] == 0) // case modifiable
                            freeCols.Add(c);
                    }

                    if (freeCols.Count > 1)
                    {
                        // Choisir deux colonnes distinctes
                        int idx1 = rand.Next(freeCols.Count);
                        int idx2 = rand.Next(freeCols.Count);
                        while (idx2 == idx1)
                            idx2 = rand.Next(freeCols.Count);

                        int col1 = freeCols[idx1];
                        int col2 = freeCols[idx2];

                        // Échange
                        int temp = voisin[row, col1];
                        voisin[row, col1] = voisin[row, col2];
                        voisin[row, col2] = temp;

                        echangeEffectue = true;
                    }
                }
                else
                {
                    // Choisir un bloc 3x3 au hasard
                    int blockRow = rand.Next(0, 3); // 0 à 2
                    int blockCol = rand.Next(0, 3);

                    int startRow = blockRow * 3;
                    int startCol = blockCol * 3;

                    // Récupérer les positions libres dans ce bloc
                    List<(int r, int c)> freeCells = new List<(int r, int c)>();
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int rr = startRow + i;
                            int cc = startCol + j;
                            if (grilleBase[rr, cc] == 0)
                            {
                                freeCells.Add((rr, cc));
                            }
                        }
                    }

                    if (freeCells.Count > 1)
                    {
                        // Choisir deux positions distinctes
                        int idx1 = rand.Next(freeCells.Count);
                        int idx2 = rand.Next(freeCells.Count);
                        while (idx2 == idx1)
                            idx2 = rand.Next(freeCells.Count);

                        var cell1 = freeCells[idx1];
                        var cell2 = freeCells[idx2];

                        // Échange
                        int temp = voisin[cell1.r, cell1.c];
                        voisin[cell1.r, cell1.c] = voisin[cell2.r, cell2.c];
                        voisin[cell2.r, cell2.c] = temp;

                        echangeEffectue = true;
                    }
                }

                tentative++;
            }

            return voisin;
        }

        /// <summary>
        /// Calcul du "coût" : nombre total de conflits 
        /// dans lignes, colonnes et blocs 3x3.
        /// </summary>
        private int CalculerCout(int[,] solution)
        {
            int conflicts = 0;

            // Lignes
            for (int row = 0; row < 9; row++)
            {
                int[] seenRow = new int[10];
                for (int col = 0; col < 9; col++)
                {
                    int val = solution[row, col];
                    seenRow[val]++;
                }
                for (int v = 1; v <= 9; v++)
                {
                    if (seenRow[v] > 1)
                        conflicts += (seenRow[v] - 1);
                }
            }

            // Colonnes
            for (int col = 0; col < 9; col++)
            {
                int[] seenCol = new int[10];
                for (int row = 0; row < 9; row++)
                {
                    int val = solution[row, col];
                    seenCol[val]++;
                }
                for (int v = 1; v <= 9; v++)
                {
                    if (seenCol[v] > 1)
                        conflicts += (seenCol[v] - 1);
                }
            }

            // Blocs 3x3
            for (int blockRow = 0; blockRow < 3; blockRow++)
            {
                for (int blockCol = 0; blockCol < 3; blockCol++)
                {
                    int[] seenBlock = new int[10];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            int row = blockRow * 3 + i;
                            int col = blockCol * 3 + j;
                            int val = solution[row, col];
                            seenBlock[val]++;
                        }
                    }
                    for (int v = 1; v <= 9; v++)
                    {
                        if (seenBlock[v] > 1)
                            conflicts += (seenBlock[v] - 1);
                    }
                }
            }

            return conflicts;
        }

        /// <summary>
        /// Copie d'une grille [9,9].
        /// </summary>
        private int[,] CopierGrille(int[,] source)
        {
            int[,] copie = new int[9, 9];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    copie[r, c] = source[r, c];
                }
            }
            return copie;
        }

        /// <summary>
        /// Convertit un SudokuGrid en int[9,9].
        /// </summary>
        private int[,] SudokuGridToArray(SudokuGrid s)
        {
            int[,] array = new int[9, 9];

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    array[row, col] = s.Cells[row, col];
                }
            }

            return array;
        }

        /// <summary>
        /// Convertit un int[9,9] en SudokuGrid.
        /// </summary>
        private SudokuGrid ArrayToSudokuGrid(int[,] array)
        {
            SudokuGrid s = new SudokuGrid();
            s.Cells = new int[9, 9];
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    s.Cells[row, col] = array[row, col];
                }
            }
            return s;
        }

        /// <summary>
        /// Mélange (shuffle) une liste en place.
        /// </summary>
        private void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = rand.Next(i, list.Count);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
using System;
using Sudoku.Shared;

namespace Sudoku.recuit_simule
{
    public class recuit : ISudokuSolver
    {
        private static readonly Random rand = new Random();

        public SudokuGrid Solve(SudokuGrid sudokuToSolve)
        {
            // 1) Convertir la grille SudokuGrid en tableau [9,9]
            int[,] grilleBase = SudokuGridToArray(sudokuToSolve);

            // 2) Paramètres du recuit simulé
            double temperatureInitiale = 2.0;
            double alpha = 0.99;
            int nbIterations = 200000;

            // 3) Lancer l'algo de recuit simulé
            var (solutionTrouvee, coutFinal) = RecuitSimuleSudoku(grilleBase, temperatureInitiale, alpha, nbIterations);

            // 4) Reconstruire un SudokuGrid depuis la solution
            SudokuGrid solvedGrid = ArrayToSudokuGrid(solutionTrouvee);

            // Optionnel : afficher le coût final
            Console.WriteLine($"Recuit terminé. Coût final : {coutFinal}");

            return solvedGrid;
        }

        /// <summary>
        /// Algorithme principal de recuit simulé pour Sudoku.
        /// </summary>
        private (int[,], int) RecuitSimuleSudoku(int[,] grilleBase, double tempInitiale, double alpha, int nbIterations)
        {
            // Générer une solution initiale
            int[,] solutionCourante = GenererSolutionInitiale(grilleBase);
            int coutCourant = CalculerCout(solutionCourante);

            // Meilleur état rencontré
            int[,] meilleureSolution = CopierGrille(solutionCourante);
            int meilleurCout = coutCourant;

            double T = tempInitiale;

            // Boucle d'itérations
            for (int iteration = 0; iteration < nbIterations; iteration++)
            {
                if (coutCourant == 0)
                    break;

                // Générer un voisin
                int[,] solutionVoisin = ChoisirVoisin(solutionCourante, grilleBase);
                int coutVoisin = CalculerCout(solutionVoisin);

                int delta = coutVoisin - coutCourant;

                if (delta < 0)
                {
                    solutionCourante = solutionVoisin;
                    coutCourant = coutVoisin;
                }
                else
                {
                    double prob = Math.Exp(-delta / T);
                    if (rand.NextDouble() < prob)
                    {
                        solutionCourante = solutionVoisin;
                        coutCourant = coutVoisin;
                    }
                }

                if (coutCourant < meilleurCout)
                {
                    meilleurCout = coutCourant;
                    meilleureSolution = CopierGrille(solutionCourante);
                }

                T *= alpha;

                if (iteration % 100 == 0)
                {
                    //Console.WriteLine($"Itération {iteration}, coût en cours = {coutCourant}, Meilleur coût = {meilleurCout}");
                }
            }

            return (meilleureSolution, meilleurCout);
        }

        /// <summary>
        /// Génère une solution initiale.
        /// </summary>
        private int[,] GenererSolutionInitiale(int[,] grilleBase)
        {
            int[,] solution = CopierGrille(grilleBase);

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (solution[row, col] == 0)
                    {
                        solution[row, col] = rand.Next(1, 10);
                    }
                }
            }

            return solution;
        }

        /// <summary>
        /// Génère un voisin.
        /// </summary>
        private int[,] ChoisirVoisin(int[,] solutionCourante, int[,] grilleBase)
        {
            int[,] voisin = CopierGrille(solutionCourante);

            while (true)
            {
                int row = rand.Next(0, 9);
                int col = rand.Next(0, 9);

                if (grilleBase[row, col] == 0)
                {
                    voisin[row, col] = rand.Next(1, 10);
                    break;
                }
            }

            return voisin;
        }

        /// <summary>
        /// Calcul du "coût".
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
    // Crée un tableau 2D pour stocker les valeurs
    int[,] array = new int[9, 9];

    for (int row = 0; row < 9; row++)
    {
        for (int col = 0; col < 9; col++)
        {
            // Accès correct à la grille en utilisant une seule paire de crochets
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
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Sudoku.Shared;

namespace Sudoku.FaresPaulSolvers
{
    public class FPYicesSimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            Console.WriteLine("🟢 Début de la résolution avec Yices");

            // 1️⃣ Générer les contraintes SMT-LIB
            string smtFilePath = "sudoku.smt2";
            string smtContent = GenerateSmtLib(s);
            File.WriteAllText(smtFilePath, smtContent);

            Console.WriteLine("🔹 Fichier SMT-LIB généré :");
            Console.WriteLine(smtContent);

            // 2️⃣ Lancer Yices
            string output = RunYices(smtFilePath);

            Console.WriteLine("🔹 Sortie de Yices :");
            Console.WriteLine(output);

            // 3️⃣ Lire la solution et remplir la grille
            SudokuGrid solvedGrid = ParseSolution(output, s);

            Console.WriteLine("🔹 Grille après parsing de la solution :");
            PrintGrid(solvedGrid);

            return solvedGrid;
        }

        private string GenerateSmtLib(SudokuGrid s)
        {
            string constraints = "(set-logic QF_LIA)\n";

            // Déclaration des variables SMT
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    constraints += $"(declare-fun x{i}{j} () Int)\n";
                    constraints += $"(assert (and (>= x{i}{j} 1) (<= x{i}{j} 9)))\n"; // Chaque case entre 1 et 9
                }
            }

            // Contraintes : chaque ligne a des valeurs distinctes
            for (int i = 0; i < 9; i++)
            {
                constraints += "(assert (distinct ";
                for (int j = 0; j < 9; j++) constraints += $" x{i}{j}";
                constraints += "))\n";
            }

            // Contraintes : chaque colonne a des valeurs distinctes
            for (int j = 0; j < 9; j++)
            {
                constraints += "(assert (distinct ";
                for (int i = 0; i < 9; i++) constraints += $" x{i}{j}";
                constraints += "))\n";
            }

            // Contraintes : chaque bloc 3x3 a des valeurs distinctes
            for (int bi = 0; bi < 3; bi++)
            {
                for (int bj = 0; bj < 3; bj++)
                {
                    constraints += "(assert (distinct ";
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            constraints += $" x{(bi * 3 + i)}{(bj * 3 + j)}";
                        }
                    }
                    constraints += "))\n";
                }
            }

            // Contraintes : respecter la grille de départ
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (s.Cells[i, j] != 0)
                    {
                        constraints += $"(assert (= x{i}{j} {s.Cells[i, j]}))\n";
                    }
                }
            }

            constraints += "(check-sat)\n(get-model)\n";
            return constraints;
        }

        private string RunYices(string smtFilePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "yices-smt2",
                Arguments = smtFilePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private SudokuGrid ParseSolution(string smtOutput, SudokuGrid s)
        {
            Regex regex = new Regex(@"\(= x(\d)(\d) (\d)\)");
            MatchCollection matches = regex.Matches(smtOutput);

            foreach (Match match in matches)
            {
                int row = int.Parse(match.Groups[1].Value);
                int col = int.Parse(match.Groups[2].Value);
                int value = int.Parse(match.Groups[3].Value);
                s.Cells[row, col] = value;
            }

            return s;
        }

        private void PrintGrid(SudokuGrid grid)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Console.Write(grid.Cells[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}

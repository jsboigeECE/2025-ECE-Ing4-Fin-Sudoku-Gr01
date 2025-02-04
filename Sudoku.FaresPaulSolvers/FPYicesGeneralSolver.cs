using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Sudoku.Shared;

namespace Sudoku.FaresPaulSolvers
{
    public abstract class BaseYicesSolver : ISudokuSolver
    {
        
        public SudokuGrid Solve(SudokuGrid s)
        {
            Console.WriteLine($"🟢 Début de la résolution avec {this.GetType().Name}");

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

            // Vérification des erreurs possibles
            if (output.Contains("unsupported"))
            {
                Console.WriteLine("⚠️ Yices a retourné 'unsupported'. Vérifiez les options SMT utilisées.");
            }

            // 3️⃣ Lire la solution et remplir la grille
            SudokuGrid solvedGrid = ParseSolution(output, s);

            Console.WriteLine("🔹 Grille après parsing de la solution :");
            PrintGrid(solvedGrid);

            return solvedGrid;
        }

        protected abstract string GetHeuristicConfig();  // 🔹 Permet aux solveurs dérivés de définir leurs heuristiques

        private string GenerateSmtLib(SudokuGrid s)
        {
            string constraints = "(set-logic QF_LIA)\n";

            // Heuristique spécifique du solveur dérivé
            string heuristics = GetHeuristicConfig();
            Console.WriteLine("🔹 Options heuristiques SMT utilisées :");
            Console.WriteLine(heuristics);
            constraints += heuristics + "\n";

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    constraints += $"(declare-fun x{i}{j} () Int)\n";
                    constraints += $"(assert (and (>= x{i}{j} 1) (<= x{i}{j} 9)))\n";
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

        private SudokuGrid ParseSolution(string smtOutput, SudokuGrid originalGrid)
        {
            SudokuGrid newGrid = new SudokuGrid(); // Nouvelle grille propre
            Array.Copy(originalGrid.Cells, newGrid.Cells, originalGrid.Cells.Length);
            
            string[] lines = smtOutput.Split('\n');
            Regex regex = new Regex(@"\(= x(\d)(\d) (\d)\)");

            bool foundSolution = false;
            foreach (string line in lines)
            {
                if (line.Contains("unsupported")) continue; // Ignorer les erreurs
                Match match = regex.Match(line);
                if (match.Success)
                {
                    int row = int.Parse(match.Groups[1].Value);
                    int col = int.Parse(match.Groups[2].Value);
                    int value = int.Parse(match.Groups[3].Value);
                    newGrid.Cells[row, col] = value;
                    Console.WriteLine($"🔹 Assignation : x{row}{col} = {value}");
                    foundSolution = true;
                }
            }

            if (!foundSolution)
            {
                Console.WriteLine("⚠️ Aucune solution valide trouvée par Yices.");
            }

            return newGrid;
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
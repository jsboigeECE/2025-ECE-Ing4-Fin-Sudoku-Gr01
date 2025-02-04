using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Sudoku.Shared;

namespace Sudoku.FaresPaulSolvers
{
    public abstract class BaseYicesSolver : ISudokuSolver
    {
        // Résout la grille Sudoku en utilisant Yices
        public SudokuGrid Solve(SudokuGrid s)
        {
            string smtFilePath = "sudoku.smt2";
            File.WriteAllText(smtFilePath, GenerateSmtLib(s));
            return ParseSolution(RunYices(smtFilePath), s);
        }

        protected abstract string GetHeuristicConfig();

        // Génère le fichier SMT-LIB avec les contraintes
        private string GenerateSmtLib(SudokuGrid s)
        {
            string constraints = "(set-logic QF_LIA)\n" + GetHeuristicConfig() + "\n";
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    constraints += $"(declare-fun x{i}{j} () Int)\n(assert (and (>= x{i}{j} 1) (<= x{i}{j} 9)))\n";
            for (int i = 0; i < 9; i++)
                constraints += "(assert (distinct " + string.Join(" ", Enumerable.Range(0, 9).Select(j => $"x{i}{j}")) + "))\n";
            for (int j = 0; j < 9; j++)
                constraints += "(assert (distinct " + string.Join(" ", Enumerable.Range(0, 9).Select(i => $"x{i}{j}")) + "))\n";
            for (int bi = 0; bi < 3; bi++)
                for (int bj = 0; bj < 3; bj++)
                    constraints += "(assert (distinct " + string.Join(" ", Enumerable.Range(0, 3).SelectMany(i => Enumerable.Range(0, 3).Select(j => $"x{bi * 3 + i}{bj * 3 + j}"))) + "))\n";
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (s.Cells[i, j] != 0)
                        constraints += $"(assert (= x{i}{j} {s.Cells[i, j]}))\n";
            return constraints + "(check-sat)\n(get-model)\n";
        }

        // Exécute Yices et récupère la sortie
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

        // Analyse la sortie SMT-LIB et met à jour la grille Sudoku
        private SudokuGrid ParseSolution(string smtOutput, SudokuGrid originalGrid)
        {
            SudokuGrid newGrid = new SudokuGrid();
            Array.Copy(originalGrid.Cells, newGrid.Cells, originalGrid.Cells.Length);
            Regex regex = new Regex(@"\(= x(\d)(\d) (\d)\)");
            foreach (string line in smtOutput.Split('\n'))
            {
                Match match = regex.Match(line);
                if (match.Success)
                    newGrid.Cells[int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value)] = int.Parse(match.Groups[3].Value);
            }
            return newGrid;
        }
    }
}

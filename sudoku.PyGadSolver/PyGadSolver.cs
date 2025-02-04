using System;
using System.Diagnostics;
using System.IO;
using Sudoku.Shared;

namespace Sudoku.PyGadSolver
{
    public class PyGadSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            string sudokuInput = ConvertGridToString(s.Cells);
            string pythonScript = "/Users/timotheolival/Documents/2025-ECE-Ing4-Fin-Sudoku-Gr01-Timoth--Mathis-Baptiste/sudoku.PyGadSolver/PyGadSolver.py";

            
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "python3",
                Arguments = $"{pythonScript} \"{sudokuInput}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process()
            {
                StartInfo = psi
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd().Trim();

            process.WaitForExit();

            return ConvertStringToGrid(result);
        }

        private string ConvertGridToString(int[,] grid)
        {
            string result = "";
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    result += grid[i, j].ToString();
                }
            }
            return result;
        }

        private SudokuGrid ConvertStringToGrid(string gridString)
{
    if (string.IsNullOrWhiteSpace(gridString))
    {
        throw new Exception("Le script Python n'a retourné aucune solution valide.");
    }

    if (!gridString.All(char.IsDigit))
    {
        throw new Exception($"La sortie du script Python contient des caractères non numériques : {gridString}");
    }

    SudokuGrid grid = new SudokuGrid();
    int[,] cells = new int[9, 9];
    int index = 0;
    for (int i = 0; i < 9; i++)
    {
        for (int j = 0; j < 9; j++)
        {
            cells[i, j] = int.Parse(gridString[index].ToString());
            index++;
        }
    }
    grid.Cells = cells;
    return grid;
}

    }
}

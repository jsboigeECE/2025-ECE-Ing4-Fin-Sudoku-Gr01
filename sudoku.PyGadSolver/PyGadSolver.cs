using System;
using System.IO;
using Python.Runtime;
using Sudoku.Shared;

namespace sudoku.PyGadSolver;

public class PyGadSolver : PythonSolverBase
{
    public override SudokuGrid Solve(SudokuGrid s)
    {
        using (PyModule scope = Py.CreateScope())
        {
            // Injectez le script de conversion
            AddNumpyConverterScript(scope);

            // Convertissez le tableau .NET en tableau NumPy
            var pyCells = AsNumpyArray(s.Cells, scope);

            // Créer une variable Python "instance"
            scope.Set("instance", pyCells);

            // Vérification du chemin du script Python
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PyGadSolver.py");
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Python script not found: {scriptPath}");
            }

            // Lire et exécuter le script Python
            string code = File.ReadAllText(scriptPath);
            scope.Exec(code);

            // Vérification que le script a bien retourné une solution
            if (!scope.Contains("solved_grid"))
            {
                throw new Exception("Python script did not return 'solved_grid'. Check for errors.");
            }

            // Récupérer la grille résolue
            PyObject result = scope.Get("solved_grid");

            // Convertir en tableau .NET
            var managedResult = AsManagedArray(scope, result);

            Console.WriteLine("Python execution completed successfully.");

            return new SudokuGrid() { Cells = managedResult };
        }
    }

    protected override void InitializePythonComponents()
    {
        InstallPipModule("numpy");
        InstallPipModule("pygad");
        base.InitializePythonComponents();
    }
}

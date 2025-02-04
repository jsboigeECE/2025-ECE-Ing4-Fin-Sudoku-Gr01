from z3 import *
from timeit import default_timer

def solve_sudoku_with_z3(grid):
    """
    Résout un Sudoku en utilisant le solveur SMT Z3 avec une approche optimisée par substitution.
    """
    # Création d'une matrice 9x9 de variables Z3
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]
    solver = Solver()

    # Contraintes de base : chaque cellule doit être entre 1 et 9
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # Contraintes d'unicité sur les lignes et colonnes
    for i in range(9):
        solver.add(Distinct(cells[i]))  # Lignes distinctes
        solver.add(Distinct([cells[j][i] for j in range(9)]))  # Colonnes distinctes

    # Contraintes d'unicité sur les sous-grilles 3x3
    for box_row in range(3):
        for box_col in range(3):
            solver.add(Distinct(
                [cells[box_row * 3 + i][box_col * 3 + j] for i in range(3) for j in range(3)]
            ))

    # Utilisation de l'API de substitution pour les valeurs fixes
    substitutions = []
    for i in range(9):
        for j in range(9):
            if grid[i][j] != 0:
                substitutions.append((cells[i][j], IntVal(grid[i][j])))

    # Application de la substitution
    solver.add([sub[0] == sub[1] for sub in substitutions])

    # Résolution du Sudoku
    start = default_timer()
    if solver.check() == sat:
        model = solver.model()
        solved_grid = [[model.evaluate(cells[i][j]).as_long() for j in range(9)] for i in range(9)]
        execution_time = (default_timer() - start) * 1000
        print(f"Le temps de résolution est de : {execution_time:.2f} ms")
        return solved_grid
    else:
        print("Aucune solution trouvée.")
        return None

# Définition de la grille de Sudoku initiale (si elle n'est pas définie par Python.NET)
if 'instance' not in locals():
    instance = [
        [0, 0, 0, 0, 9, 4, 0, 3, 0],
        [0, 0, 0, 5, 1, 0, 0, 0, 7],
        [0, 8, 9, 0, 0, 0, 0, 4, 0],
        [0, 0, 0, 0, 0, 0, 2, 0, 8],
        [0, 6, 0, 2, 0, 1, 0, 5, 0],
        [1, 0, 2, 0, 0, 0, 0, 0, 0],
        [0, 7, 0, 0, 0, 0, 5, 2, 0],
        [9, 0, 0, 0, 6, 5, 0, 0, 0],
        [0, 4, 0, 9, 7, 0, 0, 0, 0],
    ]

# Exécution de la résolution
start = default_timer()
result = solve_sudoku_with_z3(instance)
execution_time = (default_timer() - start) * 1000

# Affichage du résultat si une solution a été trouvée
if result:
    print("Sudoku résolu avec succès avec Z3 Solver:")
    for row in result:
        print(row)
    print(f"Le temps de résolution est de : {execution_time:.2f} ms")

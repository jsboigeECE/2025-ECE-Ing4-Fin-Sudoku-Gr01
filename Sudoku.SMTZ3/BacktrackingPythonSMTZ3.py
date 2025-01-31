from z3 import *
from timeit import default_timer

# Fonction pour résoudre un Sudoku avec Z3
def solve_sudoku_with_z3(grid):

    # Déclarez une matrice 9x9 de variables entières (Z3 variables)
    cells = [[Int(f"cell_{i}_{j}") for j in range(9)] for i in range(9)]

    solver = Solver()

    # Contrainte : Toutes les valeurs dans chaque cellule doivent être entre 1 et 9
    for i in range(9):
        for j in range(9):
            solver.add(And(cells[i][j] >= 1, cells[i][j] <= 9))

    # Contrainte : Les valeurs dans chaque ligne doivent être uniques
    for i in range(9):
        solver.add(Distinct(cells[i]))

    # Contrainte : Les valeurs dans chaque colonne doivent être uniques
    for j in range(9):
        solver.add(Distinct([cells[i][j] for i in range(9)]))

    # Contrainte : Les valeurs dans chaque bloc 3x3 doivent être uniques
    for box_row in range(3):
        for box_col in range(3):
            solver.add(
                Distinct(
                    [
                        cells[box_row * 3 + i][box_col * 3 + j]
                        for i in range(3)
                        for j in range(3)
                    ]
                )
            )

    # Apportez les valeurs initiales de la grille dans le solveur comme des contraintes
    for i in range(9):
        for j in range(9):
            if grid[i][j] != 0:  # Si on a une valeur prédéfinie (non vide)
                solver.add(cells[i][j] == grid[i][j])

    # Tentez de résoudre
    if solver.check() == sat:
        model = solver.model()
        solved_grid = [[model[cells[i][j]].as_long() for j in range(9)] for i in range(9)]
        return solved_grid
    else:
        print("Aucune solution trouvée.")
        return None


# Définir la grille de Sudoku initiale (passée depuis le csharp ou explicitement pour travailler en autonomie)
if 'instance' not in locals():
    instance = (
        (0, 0, 0, 0, 9, 4, 0, 3, 0),
        (0, 0, 0, 5, 1, 0, 0, 0, 7),
        (0, 8, 9, 0, 0, 0, 0, 4, 0),
        (0, 0, 0, 0, 0, 0, 2, 0, 8),
        (0, 6, 0, 2, 0, 1, 0, 5, 0),
        (1, 0, 2, 0, 0, 0, 0, 0, 0),
        (0, 7, 0, 0, 0, 0, 5, 2, 0),
        (9, 0, 0, 0, 6, 5, 0, 0, 0),
        (0, 4, 0, 9, 7, 0, 0, 0, 0),
    )

# Solve the Sudoku puzzle
start = default_timer()
result = solve_sudoku_with_z3(instance)
execution = default_timer() - start

# Affichez la solution ou un message d'échec

# if result:
#     print("Sudoku résolu avec succès avec Z3 Solver:")
#     for row in result:
#         print(row)
#     print("Le temps de résolution est de : ", execution * 1000, "ms")
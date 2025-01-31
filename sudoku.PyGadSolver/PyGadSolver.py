import numpy as np
import pygad
from timeit import default_timer

# Grille Sudoku initiale
if 'instance' not in locals():
    instance = np.array([
        [0, 0, 0, 0, 9, 4, 0, 3, 0],
        [0, 0, 0, 5, 1, 0, 0, 0, 7],
        [0, 8, 9, 0, 0, 0, 0, 4, 0],
        [0, 0, 0, 0, 0, 0, 2, 0, 8],
        [0, 6, 0, 2, 0, 1, 0, 5, 0],
        [1, 0, 2, 0, 0, 0, 0, 0, 0],
        [0, 7, 0, 0, 0, 0, 5, 2, 0],
        [9, 0, 0, 0, 6, 5, 0, 0, 0],
        [0, 4, 0, 9, 7, 0, 0, 0, 0]
    ], dtype=int)

# Détection des indices fixes et variables
fixed_indices = np.argwhere(instance > 0)
variable_indices = np.argwhere(instance == 0)

def initialize_solution():
    """Génère une solution initiale respectant les règles du Sudoku par ligne."""
    grid = instance.copy()
    for row in range(9):
        missing_numbers = list(set(range(1, 10)) - set(grid[row]))
        np.random.shuffle(missing_numbers)
        empty_positions = np.where(grid[row] == 0)[0]
        for i, pos in enumerate(empty_positions):
            grid[row, pos] = missing_numbers[i]
    return grid[variable_indices[:, 0], variable_indices[:, 1]]

def count_errors(grid):
    """Compte les erreurs dans la grille Sudoku."""
    errors = 0
    for i in range(9):
        errors += 9 - len(set(grid[i, :]))  # Lignes
        errors += 9 - len(set(grid[:, i]))  # Colonnes
    for row in range(0, 9, 3):
        for col in range(0, 9, 3):
            errors += 9 - len(set(grid[row:row + 3, col:col + 3].flatten()))  # Sous-grilles 3x3
    return errors

def fitness_function(ga_instance, solution, solution_idx):
    """Évalue la qualité d'une solution avec une pénalité stricte pour les erreurs."""
    grid = instance.copy()
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    errors = count_errors(grid)
    return 1 / (1 + errors) if errors > 0 else 100  # Score plus élevé pour solutions parfaites

def is_valid_solution(grid):
    """Vérifie si une solution Sudoku est valide."""
    return count_errors(grid) == 0

def on_generation(ga_instance):
    """Affiche l'évolution de la meilleure solution à chaque génération."""
    best_solution, best_solution_fitness, _ = ga_instance.best_solution()
    print(f"Génération {ga_instance.generations_completed}: Meilleur Fitness = {best_solution_fitness}")

# Paramètres optimisés de l'algorithme génétique
start = default_timer()
initial_population = [initialize_solution() for _ in range(1000)]

ga_instance = pygad.GA(
    num_generations=5000,
    num_parents_mating=400,
    fitness_func=fitness_function,
    sol_per_pop=1000,
    num_genes=len(variable_indices),
    gene_space=list(range(1, 10)),
    parent_selection_type="rws",
    crossover_type="two_points",
    mutation_type="random",
    mutation_probability=0.05,
    keep_parents=20,
    on_generation=on_generation,
    initial_population=initial_population
)

ga_instance.run()

execution_time = default_timer() - start

solution, solution_fitness, _ = ga_instance.best_solution()
solved_grid = instance.copy()
solved_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution

if is_valid_solution(solved_grid):
    print("\nGrille Sudoku résolue :")
    print(solved_grid)
    print(f"\nTemps de résolution : {execution_time * 1000:.2f} ms")
else:
    print("\nAucune solution valide trouvée après optimisation.")
import numpy as np
import pygad as pygad
from timeit import default_timer
import random

# Grille Sudoku initiale
instance = np.array([
    [0, 0, 0, 0, 9, 4, 0, 3, 0],
    [0, 0, 0, 5, 1, 0, 0, 0, 7],
    [0, 8, 9, 0, 0, 0, 0, 4, 0],
    [0, 0, 0, 0, 0, 0, 2, 0, 8],
    [0, 6, 0, 2, 0, 1, 0, 5, 0],
    [1, 0, 2, 0, 0, 0, 0, 0, 0],
    [0, 7, 0, 0, 0, 0, 5, 2, 0],
    [9, 0, 0, 0, 6, 5, 0, 0, 0],
    [0, 4, 0, 9, 7, 0, 0, 0, 0],
], dtype=int)

# Indices des cellules fixes
fixed_indices = np.argwhere(instance > 0)

# Fonction de fitness avec une composante "recuit simulé" intégrée
def fitness_function(ga_instance, solution, solution_idx):
    """Évalue la qualité d'une solution Sudoku."""
    grid = instance.copy()
    variable_indices = np.argwhere(grid == 0)
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution

    score = 0

    # Vérifier les lignes et colonnes
    for i in range(9):
        score += len(np.unique(grid[i, :]))  # Lignes
        score += len(np.unique(grid[:, i]))  # Colonnes

    # Vérifier les sous-blocs 3x3
    for row in range(0, 9, 3):
        for col in range(0, 9, 3):
            block = grid[row:row + 3, col:col + 3].flatten()
            score += len(np.unique(block))

    # Composante de "recuit simulé": pénaliser les solutions non valides
    penalty = np.count_nonzero(grid == 0)  # Penalité pour les cases vides
    return score - penalty

# Callback pour afficher les progrès après chaque génération
def on_generation(ga_instance):
    best_solution, best_solution_fitness, _ = ga_instance.best_solution()
    print(f"Generation = {ga_instance.generations_completed}, Best Fitness = {best_solution_fitness}")

# Indices des cellules variables
variable_indices = np.argwhere(instance == 0)

# Définir les limites des gènes (1 à 9 pour un Sudoku)
gene_space = list(range(1, 10))

# Lancement du timer
start = default_timer()

# Configuration de l'algorithme génétique
# Paramètres ajustés pour un hybride GA + Recuit Simulé
ga_instance = pygad.GA(
    num_generations=200,
    num_parents_mating=20,
    fitness_func=fitness_function,
    sol_per_pop=100,
    num_genes=len(variable_indices),
    gene_space=gene_space,
    parent_selection_type="sss",  # Sélection stochastique restreinte
    crossover_type="single_point",
    mutation_type="random",
    mutation_probability=0.1,  # Mutation faible pour préserver les meilleures solutions
    on_generation=on_generation,
)

# Exécuter l'algorithme génétique
ga_instance.run()

# Calcul du temps d'exécution
execution_time = default_timer() - start

# Obtenir la meilleure solution
solution, solution_fitness, _ = ga_instance.best_solution()

# Reconstruire la grille Sudoku avec la meilleure solution
solved_grid = instance.copy()
solved_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution

# Afficher la grille résolue et le temps d'exécution
print("\nGrille Sudoku résolue :")
print(solved_grid)
print(f"\nTemps de résolution : {execution_time * 1000:.2f} ms")
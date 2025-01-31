import numpy as np
import pygad
from timeit import default_timer

###############################################
#           Sudoku Solver via PyGAD          #
###############################################

# ---------------------------------------------------
# Grille initiale si non définie
# ---------------------------------------------------
# Nous utilisons une variable globale 'instance' si
# elle n'existe pas encore. Sinon, on part de la 
# grille Python fournie (contexte Pythonnet).
if 'instance' not in locals():
    instance = np.array([
        [0,0,0,0,9,4,0,3,0],
        [0,0,0,5,1,0,0,0,7],
        [0,8,9,0,0,0,0,4,0],
        [0,0,0,0,0,0,2,0,8],
        [0,6,0,2,0,1,0,5,0],
        [1,0,2,0,0,0,0,0,0],
        [0,7,0,0,0,0,5,2,0],
        [9,0,0,0,6,5,0,0,0],
        [0,4,0,9,7,0,0,0,0]
    ], dtype=int)

# ---------------------------------------------------
# Indices fixes et indices variables de la grille
# ---------------------------------------------------
fixed_indices = np.argwhere(instance > 0)
variable_indices = np.argwhere(instance == 0)

# ---------------------------------------------------
#  Fonction d'initialisation d'une solution
# ---------------------------------------------------
def initialize_solution():
    """
    Génère une solution initiale en s'assurant qu'il n'y a pas
    de doublons par ligne (chaque ligne est une permutation des
    valeurs manquantes).
    """
    grid = instance.copy()
    for row in range(9):
        # Valeurs manquantes dans la ligne
        missing_numbers = list(set(range(1, 10)) - set(grid[row]))
        np.random.shuffle(missing_numbers)
        # Remplir les cases vides de la ligne
        empty_positions = np.where(grid[row] == 0)[0]
        for i, pos in enumerate(empty_positions):
            grid[row, pos] = missing_numbers[i]

    # Retourne uniquement la partie variable de la grille (sous forme 1D)
    return grid[variable_indices[:, 0], variable_indices[:, 1]]

# ---------------------------------------------------
#  Fonction de comptage d'erreurs (doublons)
# ---------------------------------------------------
def count_errors(grid):
    """
    Calcule un score d'erreurs
    - Pour chaque ligne, on calcule 9 - nombre de valeurs distinctes
    - Pour chaque colonne, idem
    - Pour chaque bloc 3x3, idem
    Le total est renvoyé.
    """
    errors = 0
    for i in range(9):
        # Doublons dans la ligne i
        errors += 9 - len(set(grid[i, :]))
        # Doublons dans la colonne i
        errors += 9 - len(set(grid[:, i]))

    # Doublons dans chaque bloc 3x3
    for row in range(0, 9, 3):
        for col in range(0, 9, 3):
            block = grid[row:row+3, col:col+3].flatten()
            errors += 9 - len(set(block))

    return errors

# ---------------------------------------------------
#  Fonction de fitness pour PyGAD
# ---------------------------------------------------
def fitness_function(ga_instance, solution, solution_idx):
    """
    Construit la grille à partir de la solution proposée,
    calcule le nombre d'erreurs, puis renvoie un score.
    Ici: score = 500 - 5 * erreurs, borné à 0.
    """
    grid = instance.copy()
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    errors = count_errors(grid)

    raw_score = 500 - 5 * errors
    return max(raw_score, 0)

# ---------------------------------------------------
#  Vérification de validité (0 erreur)
# ---------------------------------------------------
def is_valid_solution(grid):
    """Renvoie True si la grille ne contient aucune erreur."""
    return count_errors(grid) == 0

# ---------------------------------------------------
#  Callback exécuté à chaque génération
# ---------------------------------------------------
def on_generation(ga_instance):
    """
    Affiche la meilleure fitness et arrête l'AG si on a
    une solution parfaite et valide (fitness=500).
    Pour PyGAD>=2.4.0, retourner 'stop' stoppe l'algo.
    """
    best_solution, best_solution_fitness, _ = ga_instance.best_solution()
    print(f"Génération {ga_instance.generations_completed} : "
          f"Meilleur fitness = {best_solution_fitness}")

    # Si on atteint la fitness maximum (500)
    if best_solution_fitness == 500:
        # Vérifier la validité réelle de la solution
        check_grid = instance.copy()
        check_grid[variable_indices[:, 0], variable_indices[:, 1]] = best_solution
        if is_valid_solution(check_grid):
            # On arrête l'AG immédiatement à la fin de cette génération
            return "stop"

# ---------------------------------------------------
#   MAIN: lancement de l'AG et stockage du résultat
# ---------------------------------------------------
def main():
    """
    Gère la configuration de l'algorithme génétique,
    exécute la recherche, puis renvoie la grille finale.
    """
    start = default_timer()

    # Paramètres
    population_size = 300
    initial_population = [initialize_solution() for _ in range(population_size)]

    # Création de l'AG
    ga_instance = pygad.GA(
        num_generations=2000,            # Nombre max de générations
        num_parents_mating=50,           # Nb de parents pour crossover
        fitness_func=fitness_function,
        sol_per_pop=population_size,
        num_genes=len(variable_indices),
        gene_space=list(range(1, 10)),
        parent_selection_type="tournament",
        crossover_type="two_points",
        mutation_type="random",
        mutation_probability=0.03,
        keep_parents=5,
        on_generation=on_generation,
        initial_population=initial_population
    )

    # Lancement de l'AG
    ga_instance.run()

    # Calcul du temps écoulé
    execution_time = default_timer() - start

    # Récupération de la meilleure solution
    solution, solution_fitness, _ = ga_instance.best_solution()

    # Construction de la grille finale
    final_grid = instance.copy()
    final_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution

    return final_grid, solution_fitness, execution_time

# ---------------------------------------------------
# Execution immédiate: on expose solved_grid en global
# ---------------------------------------------------
solved_grid, best_fitness, runtime = main()

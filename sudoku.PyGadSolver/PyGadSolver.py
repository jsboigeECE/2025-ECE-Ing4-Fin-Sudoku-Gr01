import numpy as np
import pygad
from timeit import default_timer

###############################################
#     Sudoku Solver optimisé via PyGAD       #
###############################################

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

fixed_indices = np.argwhere(instance > 0)
variable_indices = np.argwhere(instance == 0)

# Génération initiale améliorée avec des permutations valides

def initialize_solution():
    grid = instance.copy()
    for row in range(9):
        empty_positions = np.where(grid[row] == 0)[0]
        missing_numbers = list(set(range(1, 10)) - set(grid[row]))
        np.random.shuffle(missing_numbers)
        for i, pos in enumerate(empty_positions):
            grid[row, pos] = missing_numbers[i]
    return grid[variable_indices[:, 0], variable_indices[:, 1]]

# Fonction de comptage des erreurs améliorée

def count_errors(grid):
    errors = 0
    for i in range(9):
        errors += (9 - len(set(grid[i, :])))  # Lignes
        errors += (9 - len(set(grid[:, i])))  # Colonnes
    for row in range(0, 9, 3):
        for col in range(0, 9, 3):
            block = grid[row:row+3, col:col+3].flatten()
            errors += (9 - len(set(block)))  # Blocs 3x3
    return errors

# Nouvelle fonction de fitness basée sur la distance à une solution valide

def fitness_function(ga_instance, solution, solution_idx):
    grid = instance.copy()
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    errors = count_errors(grid)
    return max(100 - 10 * errors, 0)  # Ajustement de la pénalisation pour améliorer la convergence

# Fonction d'arrêt basée sur la stabilisation du score
def on_generation(ga_instance):
    best_solution, best_solution_fitness, _ = ga_instance.best_solution()
    print(f"Génération {ga_instance.generations_completed} : Meilleur fitness = {best_solution_fitness}")
    if best_solution_fitness >= 98:  # Seuil pour arrêter
        check_grid = instance.copy()
        check_grid[variable_indices[:, 0], variable_indices[:, 1]] = best_solution
        if count_errors(check_grid) == 0:
            return "stop"

# Croisement plus structuré pour préserver la validité des solutions
def structured_crossover(parents, offspring_size, ga_instance):
    offspring = []
    for _ in range(offspring_size[0]):
        parent1, parent2 = np.random.choice(parents.shape[0], 2, replace=False)
        cut1, cut2 = sorted(np.random.choice(len(parents[parent1]), 2, replace=False))
        child = np.concatenate((parents[parent1][:cut1], parents[parent2][cut1:cut2], parents[parent1][cut2:]))
        offspring.append(child)
    return np.array(offspring)

# Mutation améliorée avec des échanges ciblés
def guided_mutation(offspring, ga_instance):
    mutation_rate = 0.05  # Réduction pour limiter la destruction des solutions
    for idx in range(len(offspring)):
        if np.random.rand() < mutation_rate:
            pos1, pos2 = np.random.choice(len(offspring[idx]), 2, replace=False)
            offspring[idx][pos1], offspring[idx][pos2] = offspring[idx][pos2], offspring[idx][pos1]
    return offspring

# Exécution optimisée de l'algorithme génétique
def main():
    start = default_timer()
    population_size = 500
    initial_population = [initialize_solution() for _ in range(population_size)]
    
    ga_instance = pygad.GA(
        num_generations=2500,
        num_parents_mating=100,
        fitness_func=fitness_function,
        sol_per_pop=population_size,
        num_genes=len(variable_indices),
        gene_space=list(range(1, 10)),
        parent_selection_type="tournament",
        crossover_type=structured_crossover,
        mutation_type=guided_mutation,
        keep_parents=15,
        on_generation=on_generation,
        initial_population=initial_population
    )
    
    ga_instance.run()
    execution_time = default_timer() - start
    solution, solution_fitness, _ = ga_instance.best_solution()
    final_grid = instance.copy()
    final_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    return final_grid, solution_fitness, execution_time

solved_grid, best_fitness, runtime = main()

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

# Vérification de validité pour éviter les mutations destructrices
def is_valid(grid, row, col, num):
    if num in grid[row]:
        return False
    if num in grid[:, col]:
        return False
    start_row, start_col = 3 * (row // 3), 3 * (col // 3)
    for r in range(start_row, start_row + 3):
        for c in range(start_col, start_col + 3):
            if grid[r, c] == num:
                return False
    return True

# Génération initiale améliorée basée sur la logique du backtracking
def initialize_solution():
    grid = instance.copy()
    for row in range(9):
        empty_positions = np.where(grid[row] == 0)[0]
        missing_numbers = list(set(range(1, 10)) - set(grid[row]))
        np.random.shuffle(missing_numbers)
        for i, pos in enumerate(empty_positions):
            grid[row, pos] = missing_numbers[i]
    return grid[variable_indices[:, 0], variable_indices[:, 1]]

# Fonction de fitness ajustée pour une meilleure convergence
def fitness_function(ga_instance, solution, solution_idx):
    grid = instance.copy()
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    errors = sum([not is_valid(grid, row, col, grid[row, col]) for row, col in variable_indices])
    return max(500 - 20 * errors, 0)

# Croisement respectant la structure correcte du Sudoku
def structured_crossover(parents, offspring_size, ga_instance):
    offspring = []
    for _ in range(offspring_size[0]):
        parent1, parent2 = np.random.choice(parents.shape[0], 2, replace=False)
        cut1, cut2 = sorted(np.random.choice(len(parents[parent1]), 2, replace=False))
        child = np.concatenate((parents[parent1][:cut1], parents[parent2][cut1:cut2], parents[parent1][cut2:]))
        offspring.append(child)
    return np.array(offspring)

# Mutation contrôlée basée sur la validité du Sudoku
def guided_mutation(offspring, ga_instance):
    mutation_rate = 0.05
    for idx in range(len(offspring)):
        if np.random.rand() < mutation_rate:
            pos1, pos2 = np.random.choice(len(offspring[idx]), 2, replace=False)
            if is_valid(instance, variable_indices[pos1][0], variable_indices[pos1][1], offspring[idx][pos2]) and \
               is_valid(instance, variable_indices[pos2][0], variable_indices[pos2][1], offspring[idx][pos1]):
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
        initial_population=initial_population
    )
    
    ga_instance.run()
    execution_time = default_timer() - start
    solution, solution_fitness, _ = ga_instance.best_solution()
    final_grid = instance.copy()
    final_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    return final_grid, solution_fitness, execution_time

solved_grid, best_fitness, runtime = main()

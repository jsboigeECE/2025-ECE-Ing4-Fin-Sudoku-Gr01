import numpy as np
import pygad
import random
from copy import deepcopy
from timeit import default_timer

# Si 'instance' n'est pas défini, définir une grille par défaut
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

N = 9
fixed_indices = np.argwhere(instance > 0)
variable_indices = np.argwhere(instance == 0)

def is_valid(grid, row, col, num):
    if num in grid[row] or num in grid[:, col]:
        return False
    start_row, start_col = 3 * (row // 3), 3 * (col // 3)
    if num in grid[start_row:start_row + 3, start_col:start_col + 3]:
        return False
    return True

def fitness_function(ga_instance, solution, solution_idx):
    grid = instance.copy()
    grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    errors = sum([not is_valid(grid, row, col, grid[row, col]) for row, col in variable_indices])
    return max(500 - 20 * errors, 0)

def structured_crossover(parents, offspring_size, ga_instance):
    offspring = []
    for _ in range(offspring_size[0]):
        parent1, parent2 = np.random.choice(parents.shape[0], 2, replace=False)
        cut1, cut2 = sorted(np.random.choice(len(parents[parent1]), 2, replace=False))
        child = np.concatenate((parents[parent1][:cut1], parents[parent2][cut1:cut2], parents[parent1][cut2:]))
        offspring.append(child)
    return np.array(offspring)

def guided_mutation(offspring, ga_instance):
    mutation_rate = 0.2
    for idx in range(len(offspring)):
        if np.random.rand() < mutation_rate:
            pos1, pos2 = np.random.choice(len(offspring[idx]), 2, replace=False)
            offspring[idx][pos1], offspring[idx][pos2] = offspring[idx][pos2], offspring[idx][pos1]
    return offspring

def solve_with_backtracking(grid):
    empty = np.argwhere(grid == 0)
    if empty.size == 0:
        return True
    row, col = empty[0]
    for num in range(1, N + 1):
        if is_valid(grid, row, col, num):
            grid[row][col] = num
            if solve_with_backtracking(grid):
                return True
            grid[row][col] = 0
    return False

def propagation_contraintes(grid):
    changes = True
    while changes:
        changes = False
        for row in range(N):
            for col in range(N):
                if grid[row][col] == 0:
                    possibles = {i for i in range(1, 10) if is_valid(grid, row, col, i)}
                    if len(possibles) == 1:
                        grid[row][col] = possibles.pop()
                        changes = True

def main():
    start = default_timer()
    population_size = 300
    num_generations = 3000
    keep_parents = 100
    initial_population = [np.random.randint(1, 10, len(variable_indices)) for _ in range(population_size)]
    
    ga_instance = pygad.GA(
        num_generations=num_generations,
        num_parents_mating=150,
        fitness_func=fitness_function,
        sol_per_pop=population_size,
        num_genes=len(variable_indices),
        gene_space=list(range(1, 10)),
        parent_selection_type="tournament",
        crossover_type=structured_crossover,
        mutation_type=guided_mutation,
        keep_parents=keep_parents,
        initial_population=initial_population
    )
    
    ga_instance.run()
    solution, solution_fitness, _ = ga_instance.best_solution()
    final_grid = instance.copy()
    final_grid[variable_indices[:, 0], variable_indices[:, 1]] = solution
    
    propagation_contraintes(final_grid)
    if not solve_with_backtracking(final_grid):
        print("Backtracking required but failed to resolve the Sudoku")
    
    execution_time = default_timer() - start
    print(f"Fitness finale : {solution_fitness}")
    print("Grille résolue :")
    print(final_grid)
    
    return final_grid, solution_fitness, execution_time

solved_grid, best_fitness, runtime = main()

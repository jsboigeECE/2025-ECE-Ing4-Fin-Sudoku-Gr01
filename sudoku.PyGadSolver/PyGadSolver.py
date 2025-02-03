import numpy as np
import pygad
import random
from copy import deepcopy
from timeit import default_timer

###############################################
#     Sudoku Solver via Algorithme Génétique #
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
N = 9

# Vérification de validité
def is_valid(grid, row, col, num):
    if num in grid[row] or num in grid[:, col]:
        return False
    start_row, start_col = 3 * (row // 3), 3 * (col // 3)
    if num in grid[start_row:start_row+3, start_col:start_col+3]:
        return False
    return True

# Calcul de la fitness
def fitness(grid):
    score = 0
    for row in grid:
        score += len(set(row))
    for col in grid.T:
        score += len(set(col))
    return score

# Génération initiale
def generate_population(grid, population_size):
    population = []
    for _ in range(population_size):
        new_grid = deepcopy(grid)
        for row in range(N):
            missing_numbers = [num for num in range(1, N+1) if num not in new_grid[row]]
            random.shuffle(missing_numbers)
            for col in range(N):
                if new_grid[row][col] == 0:
                    new_grid[row][col] = missing_numbers.pop()
        population.append(new_grid)
    return population

# Croisement
def crossover(parent1, parent2):
    child = deepcopy(parent1)
    for row in range(N):
        if random.random() > 0.5:
            child[row] = parent2[row]
    return child

# Mutation
def mutate(grid, mutation_rate=0.1):
    new_grid = deepcopy(grid)
    for row in range(N):
        if random.random() < mutation_rate:
            cols = [col for col in range(N) if new_grid[row][col] == 0]
            if len(cols) > 1:
                col1, col2 = random.sample(cols, 2)
                new_grid[row][col1], new_grid[row][col2] = new_grid[row][col2], new_grid[row][col1]
    return new_grid

# Algorithme Génétique
def genetic_algorithm_solver(initial_grid, population_size=500, generations=3000, mutation_rate=0.1):
    population = generate_population(initial_grid, population_size)
    best_solution = None
    best_fitness = 0
    
    for generation in range(generations):
        population = sorted(population, key=lambda x: fitness(x), reverse=True)
        if fitness(population[0]) > best_fitness:
            best_fitness = fitness(population[0])
            best_solution = population[0]
        if best_fitness == N * 2 * N:
            break
        next_generation = population[:population_size//10]
        for _ in range(population_size - population_size//10):
            parent1, parent2 = random.sample(population[:population_size//2], 2)
            child = crossover(parent1, parent2)
            child = mutate(child, mutation_rate)
            next_generation.append(child)
        population = next_generation
        if generation % 500 == 0:
            print(f"Génération {generation}: Meilleure Fitness = {best_fitness}")
    
    return best_solution

# Fonction principale
def main():
    start = default_timer()
    initial_grid = np.array(instance)
    solved_grid = genetic_algorithm_solver(initial_grid)
    execution_time = default_timer() - start
    
    print("Grille Résolue :")
    print(solved_grid)
    
    return solved_grid, fitness(solved_grid), execution_time

solved_grid, best_fitness, runtime = main()

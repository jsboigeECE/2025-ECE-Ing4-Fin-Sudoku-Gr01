import numpy as np
import pygad
import sys

def parse_grid(grid_str):
    """Convertit une grille sous forme de string en une matrice numpy 9x9."""
    if not grid_str or len(grid_str) != 81:
        raise ValueError("Grille invalide : elle doit contenir exactement 81 chiffres.")
    return np.array([int(c) for c in grid_str]).reshape(9, 9)

def fitness_function(ga_instance, solution, solution_idx):
    """Évalue la qualité d'une solution de Sudoku."""
    grid = np.round(solution).astype(int).reshape(9, 9)
    score = 0
    for i in range(9):
        score -= (9 - len(set(grid[i, :]))) * 10  # Pénalité pour répétition en ligne
        score -= (9 - len(set(grid[:, i]))) * 10  # Pénalité pour répétition en colonne
    for r in range(0, 9, 3):
        for c in range(0, 9, 3):
            score -= (9 - len(set(grid[r:r+3, c:c+3].flatten()))) * 10  # Pénalité pour répétition en bloc 3x3
    return score

def solve_sudoku(grid):
    """Méthode hybride : essaie PyGAD puis améliore la solution avec le backtracking."""
    solution = solve_with_pygad(grid)
    if solution is None:
        return solve_with_backtracking(grid)
    return refine_solution_with_backtracking(grid, solution)

def solve_with_pygad(grid):
    """Résout le Sudoku avec PyGAD."""
    initial_values = grid.flatten()
    gene_space = [
        list(range(1, 10)) if initial_values[i] == 0 else [initial_values[i]]
        for i in range(81)
    ]
    ga_instance = pygad.GA(
        num_generations=500,
        num_parents_mating=50,
        fitness_func=fitness_function,
        sol_per_pop=300,
        num_genes=81,
        gene_space=gene_space,
        parent_selection_type="sss",
        keep_parents=10,
        mutation_percent_genes=15,
        crossover_type="uniform",
        stop_criteria=["reach_0"],
    )
    ga_instance.run()
    best_solution, _, _ = ga_instance.best_solution()
    solution_grid = np.round(np.array(best_solution)).astype(int).reshape(9, 9)
    return solution_grid if is_valid_solution(solution_grid) else None

def refine_solution_with_backtracking(original_grid, partial_solution):
    """Corrige la solution incomplète de PyGAD avec le backtracking."""
    def is_valid(board, row, col, num):
        if num in board[row, :] or num in board[:, col]:
            return False
        r, c = 3 * (row // 3), 3 * (col // 3)
        if num in board[r:r+3, c:c+3]:
            return False
        return True
    
    def solve(board):
        for row in range(9):
            for col in range(9):
                if original_grid[row, col] == 0 and not is_valid_solution(board):
                    for num in range(1, 10):
                        if is_valid(board, row, col, num):
                            board[row, col] = num
                            if solve(board):
                                return True
                            board[row, col] = 0
                    return False
        return True
    
    refined_grid = partial_solution.copy()
    solve(refined_grid)
    return refined_grid

def solve_with_backtracking(grid):
    """Solveur de secours : résolution par backtracking classique."""
    def is_valid(board, row, col, num):
        if num in board[row, :] or num in board[:, col]:
            return False
        r, c = 3 * (row // 3), 3 * (col // 3)
        if num in board[r:r+3, c:c+3]:
            return False
        return True
    
    def solve(board):
        for row in range(9):
            for col in range(9):
                if board[row, col] == 0:
                    for num in range(1, 10):
                        if is_valid(board, row, col, num):
                            board[row, col] = num
                            if solve(board):
                                return True
                            board[row, col] = 0
                    return False
        return True
    
    grid_copy = grid.copy()
    if solve(grid_copy):
        return grid_copy
    return None

def is_valid_solution(grid):
    """Vérifie si une grille de Sudoku est valide."""
    for i in range(9):
        if len(set(grid[i, :])) != 9 or len(set(grid[:, i])) != 9:
            return False
    for r in range(0, 9, 3):
        for c in range(0, 9, 3):
            if len(set(grid[r:r+3, c:c+3].flatten())) != 9:
                return False
    return True

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Erreur: Aucun argument de grille fourni.", file=sys.stderr)
        sys.exit(1)
    input_grid = sys.argv[1]
    grid = parse_grid(input_grid)
    solution = solve_sudoku(grid)
    if solution is not None:
        print(''.join(map(str, solution.flatten())))
    else:
        print("0" * 81)  # Retourne une grille remplie de zéros en cas d'échec.

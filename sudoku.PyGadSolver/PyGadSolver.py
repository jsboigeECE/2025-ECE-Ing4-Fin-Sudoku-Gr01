import numpy as np
import pygad
import sys

# 📌 Étape 1 : Conversion de la grille Sudoku sous forme de chaîne en matrice NumPy 9x9
def parse_grid(grid_str):
    """Convertit une grille sous forme de string en une matrice numpy 9x9."""
    
    # Vérifie que la chaîne d'entrée contient exactement 81 chiffres (grille complète)
    if not grid_str or len(grid_str) != 81:
        raise ValueError("Grille invalide : elle doit contenir exactement 81 chiffres.")
    
    # Convertit la chaîne en une liste d'entiers et la reformate en une matrice 9x9
    return np.array([int(c) for c in grid_str]).reshape(9, 9)


# 📌 Étape 2 : Fonction de fitness qui évalue la qualité d'une grille de Sudoku
def fitness_function(ga_instance, solution, solution_idx):
    """Évalue la qualité d'une solution de Sudoku."""

    # Reformate la solution en grille 9x9
    grid = np.round(solution).astype(int).reshape(9, 9)
    score = 0

    # Vérifie et pénalise les erreurs dans les lignes
    for i in range(9):
        score -= (9 - len(set(grid[i, :]))) * 10  # Moins il y a de doublons, meilleur est le score
        score -= (9 - len(set(grid[:, i]))) * 10  # Idem pour les colonnes

    # Vérifie et pénalise les erreurs dans les sous-blocs 3x3
    for r in range(0, 9, 3):
        for c in range(0, 9, 3):
            score -= (9 - len(set(grid[r:r+3, c:c+3].flatten()))) * 10  # Même logique pour les blocs 3x3

    return score  # Plus le score est proche de 0, plus la grille est correcte


# 📌 Étape 3 : Résolution du Sudoku en combinant PyGAD et backtracking
def solve_sudoku(grid):
    """Méthode hybride : essaie PyGAD, puis améliore la solution avec le backtracking."""
    
    # Utilise l'algorithme génétique PyGAD pour résoudre la grille
    solution = solve_with_pygad(grid)
    
    # Si PyGAD échoue à produire une solution valide, on utilise le backtracking
    if solution is None:
        return solve_with_backtracking(grid)
    
    # Si PyGAD trouve une solution mais imparfaite, on l'affine avec le backtracking
    return refine_solution_with_backtracking(grid, solution)


# 📌 Étape 4 : Application de l’algorithme génétique PyGAD
def solve_with_pygad(grid):
    """Résout le Sudoku avec PyGAD."""

    # Conversion de la grille en un tableau linéaire (flatten)
    initial_values = grid.flatten()

    # Définition des valeurs possibles pour chaque case (1-9 si vide, valeur fixe sinon)
    gene_space = [
        list(range(1, 10)) if initial_values[i] == 0 else [initial_values[i]]
        for i in range(81)
    ]

    # Initialisation et configuration de l’algorithme génétique
    ga_instance = pygad.GA(
        num_generations=500,  # Nombre maximum de générations
        num_parents_mating=50,  # Nombre de parents sélectionnés pour la reproduction
        fitness_func=fitness_function,  # Fonction de scoring (fitness)
        sol_per_pop=300,  # Nombre d'individus dans la population
        num_genes=81,  # Chaque grille a 81 gènes (cases)
        gene_space=gene_space,  # Espaces des gènes (valeurs possibles)
        parent_selection_type="sss",  # Sélection des parents basée sur le score (Stochastic Universal Sampling)
        keep_parents=10,  # Nombre de parents conservés entre les générations
        mutation_percent_genes=15,  # Pourcentage de cellules modifiées lors de la mutation
        crossover_type="uniform",  # Type de croisement : les cellules peuvent provenir de différents parents
        stop_criteria=["reach_0"],  # Arrêt lorsque la solution correcte est trouvée (score = 0)
    )

    # Exécution de l’algorithme génétique
    ga_instance.run()

    # Récupération de la meilleure solution trouvée
    best_solution, _, _ = ga_instance.best_solution()
    solution_grid = np.round(np.array(best_solution)).astype(int).reshape(9, 9)

    # Vérification si la solution est correcte
    return solution_grid if is_valid_solution(solution_grid) else None


# 📌 Étape 5 : Correction avec le backtracking si nécessaire
def refine_solution_with_backtracking(original_grid, partial_solution):
    """Corrige une solution incomplète de PyGAD avec le backtracking."""

    def is_valid(board, row, col, num):
        """Vérifie si un nombre est valide dans une cellule donnée."""
        if num in board[row, :] or num in board[:, col]:
            return False
        r, c = 3 * (row // 3), 3 * (col // 3)
        if num in board[r:r+3, c:c+3]:
            return False
        return True

    def solve(board):
        """Applique le backtracking pour ajuster la grille."""
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


# 📌 Étape 6 : Implémentation du solveur backtracking classique
def solve_with_backtracking(grid):
    """Solveur de secours : résolution par backtracking classique."""

    def is_valid(board, row, col, num):
        """Vérifie si un nombre est valide dans une cellule donnée."""
        if num in board[row, :] or num in board[:, col]:
            return False
        r, c = 3 * (row // 3), 3 * (col // 3)
        if num in board[r:r+3, c:c+3]:
            return False
        return True

    def solve(board):
        """Applique la résolution complète par backtracking."""
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


# 📌 Étape 7 : Vérification finale de la solution
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


# 📌 Étape 8 : Exécution du programme en ligne de commande
if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Erreur: Aucun argument de grille fourni.", file=sys.stderr)
        sys.exit(1)

    # Lecture de la grille Sudoku en argument
    input_grid = sys.argv[1]
    grid = parse_grid(input_grid)

    # Résolution du Sudoku
    solution = solve_sudoku(grid)

    # Affichage du résultat final
    if solution is not None:
        print(''.join(map(str, solution.flatten())))
    else:
        print("0" * 81)  # Retourne une grille remplie de zéros si échec

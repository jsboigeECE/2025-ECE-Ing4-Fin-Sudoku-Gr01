# Projet Solveur de Sudoku avec Choco Solver

Ce projet implémente plusieurs solveurs pour le Sudoku en utilisant la librairie [Choco Solver](https://choco-solver.org/), un moteur de résolution de contraintes en Java. L'objectif est d'explorer différentes stratégies de résolution en s'appuyant sur les fonctionnalités avancées de Choco pour la modélisation et la résolution de problèmes de satisfaction de contraintes (CSP).



## Présentation de Choco Solver

Choco Solver est une librairie open source dédiée à la programmation par contraintes. Elle permet de :

- **Modéliser** des problèmes complexes en définissant des variables, des domaines et des contraintes.
- **Résoudre** ces problèmes grâce à des stratégies de recherche et de propagation de contraintes.
- **Optimiser** la recherche de solution en utilisant des heuristiques avancées (ex. : First Fail, DomOverWDeg).



Dans ce projet, Choco Solver est utilisé pour résoudre le Sudoku en imposant les contraintes classiques d'unicité sur les lignes, colonnes et blocs (sous-grilles 3×3).


## Structure du Projet

Le projet se compose de plusieurs classes qui implémentent différentes approches de résolution :

### 1. AbstractChocoSolver

- **Rôle :**
Classe de base abstraite qui définit la structure commune pour tous les solveurs de Sudoku utilisant Choco.

-**Méthodes principales :**
- `Solve(SudokuGrid grid)`: 
    Valide la grille d'entrée, crée le modèle Choco, définit les variables et les contraintes (lignes, colonnes, blocs), puis extrait la solution.

- `ValidateInput(SudokuGrid grid)`:  
    Vérifie qu'il n'y a pas de doublons dans les lignes, colonnes ou blocs de la grille initiale.

- `CreateCellVariables(Model model, SudokuGrid grid)`:  
    Crée des variables pour chaque cellule du Sudoku, en fixant les valeurs préexistantes et en définissant le domaine [1,9] pour les cellules vides.

- `ApplyConstraints(Model model, IntVar[][] cellVariables)`:  
    Applique les contraintes d'unicité pour les lignes, colonnes et sous-grilles.

- `GetSolver(Model model, IntVar[][] cellVariables)`:  
    Méthode virtuelle permettant de configurer le solveur avec des stratégies de recherche avancées.

- `ExtractSolution(SudokuGrid grid, IntVar[][] cells)`:  
    Récupère la solution trouvée par Choco et la copie dans la grille de Sudoku.



### 2. ChocoDegreePropagationSolver

- **Rôle :**  

  Utilise une stratégie basée sur l'heuristique du degré (Degree Heuristic) associée à une propagation avancée des contraintes.

- **Méthodes principales :**

- `ConfigureSearch(Model model, IntVar[][] cells)`:  
    Configure la recherche en utilisant l'heuristique `DomOverWDeg` pour la sélection des variables et `IntDomainMin` pour le choix des valeurs.
- `ApplyConstraints(Model model, IntVar[][] cells)`:  
    Hérite des contraintes de base de `AbstractChocoSolver` et ajoute une propagation des "Naked Singles" en instanciant automatiquement les cellules dont le domaine est réduit à une seule valeur.

### 3. ChocoSimpleSolver

- **Rôle :**  
  Propose une approche simple de résolution en créant un modèle de contraintes classique et en postant l'ensemble des contraintes (lignes, colonnes, blocs) ainsi que les valeurs initiales.

- **Méthodes principales :**
  - `Solve(SudokuGrid s)`:  
    Crée le modèle, définit les variables et les contraintes, puis lance la résolution et extrait la solution.
  - `GetSolver(Model model, IntVar[][] cellVariables, List<Constraint> constraints)`:  
    Configure le solveur en postant l'ensemble des contraintes sur le modèle.

### 4. ChocoDomOverWDegRefSearchSolver

- **Rôle :**  
  Étend la logique du `ChocoSimpleSolver` en appliquant une stratégie de recherche avancée basée sur `domOverWDegRefSearch`.

- **Méthodes principales :**
  - `GetSolver(Model model, IntVar[][] cellVariables, List<Constraint> constraints)`:  
    Configure le solveur en postant les contraintes et en appliquant la stratégie `domOverWDegRefSearch` sur les variables aplaties.

### 5. ChocoSolverVariableSelector

- **Rôle :**  
  Implémente un solveur optimisé en utilisant des sélecteurs de variables et de valeurs avancés.

- **Méthodes principales :**
  - `GetSolver(Model model, IntVar[][] cells)`:  
    Configure la stratégie de recherche avec :
    - **First Fail** : Sélectionne la variable avec le domaine le plus petit.
    - **IntDomainMin** : Choisit la plus petite valeur disponible.
    
    De plus, la stratégie de redémarrage est désactivée pour éviter des interruptions coûteuses lors de la recherche d'une solution.

# Solveur de Sudoku avec Dancing Links (DLX)

## Description

Ce projet implémente un solveur de Sudoku en C# en utilisant la technique de **Dancing Links (DLX)**, une approche efficace pour résoudre des problèmes de **couverture exacte**. L'algorithme X de **Donald Knuth** est utilisé pour explorer systématiquement les solutions possibles de manière optimisée.

L'objectif est d'implémenter un solveur performant capable de résoudre des Sudokus de différentes tailles en exploitant la flexibilité de l'algorithme DLX et en y ajoutant des heuristiques avancées pour améliorer les performances.

## Structure du Projet

Le projet est composé des classes suivantes :

### 1. `DancingLinksSolverBase`

- Classe **abstraite** qui sert de base aux solveurs spécifiques.
- Contient les méthodes permettant de **convertir un Sudoku en matrice de couverture exacte**.
- Fournit un mécanisme pour reconstruire la grille Sudoku à partir de la solution renvoyée par DLX.
- Contient des utilitaires pour **extraire et vérifier** la validité des lignes, colonnes et blocs d'un Sudoku.
- Définit la méthode abstraite `Solve(SudokuGrid s)` qui est ensuite implémentée dans les solveurs dérivés.

**Technique principale :**

- **Transformation en couverture exacte** :
  - Chaque cellule du Sudoku est représentée par une **contrainte** dans une matrice binaire.
  - Une ligne de cette matrice représente un placement potentiel d'un chiffre dans une cellule spécifique.
  - Les contraintes incluent : une seule valeur par cellule, chaque chiffre unique dans une ligne, une colonne et un bloc.

### 2. `DancingLinksSolverSimple`

- Implémente un solveur basique en utilisant **Dancing Links (DLX)** sans heuristique avancée.
- Utilise la bibliothèque **DlxLib** pour appliquer l'algorithme de **Donald Knuth** et résoudre le problème de couverture exacte.
- Convertit la grille Sudoku en une matrice binaire représentant les contraintes et applique l'algorithme DLX pour trouver une solution.
- Affiche la solution trouvée ou indique si aucune solution n'est disponible.

**Points techniques :**

- La recherche de solutions est effectuée via une **recherche en profondeur récursive** avec élimination des options non valides.
- Utilisation de `FirstOrDefault()` pour récupérer uniquement la première solution viable.

### 3. `DancingLinksSolverMRV`

- Version avancée du solveur utilisant l'heuristique **Minimum Remaining Values (MRV)** pour améliorer l'efficacité de la recherche.
- Sélectionne en priorité la colonne contenant **le moins d'options disponibles**, réduisant ainsi la taille de l'espace de recherche.
- Applique une approche récursive en sélectionnant les lignes candidates ayant des **1** dans la colonne avec **le moins de choix possibles**.
- Filtrage des lignes incompatibles pour accélérer le processus de backtracking.

**Points techniques avancés :**

- **Sélection adaptative des colonnes** :
  - Chaque colonne représente une contrainte.
  - L'heuristique MRV sélectionne celle avec le **moins de candidats possibles** pour réduire le nombre de combinaisons à tester.
- **Filtrage des matrices** :
  - Suppression des colonnes satisfaites après le choix d'une ligne candidate.
  - Élimination des lignes en conflit pour éviter les incohérences dans la solution.

## Dépendances

Le projet repose sur les bibliothèques suivantes :

- **`DlxLib`** : Implémentation de l'algorithme **Dancing Links**.
- **`Sudoku.Shared`** : Définition des structures de données pour représenter une grille de Sudoku et manipuler ses valeurs.

## Installation et Utilisation

1. **Cloner le dépôt** contenant ce projet :
   ```sh
   git clone https://github.com/Tenzincmoi/2025-ECE-Ing4-Fin-Sudoku-DancingLinksTenzin.git
   ```
2. **Ajouter la bibliothèque `DlxLib` via NuGet** si elle n'est pas déjà installée :
   ```sh
   dotnet add package DlxLib
   ```
3. **Compiler** le projet pour vérifier les erreurs :
   ```sh
   dotnet build
   ```


## Auteur

- Projet développé dans le cadre d'un **cours de résolution de Sudoku** avec l'algorithme **Dancing Links** en **C#**.
- Implémentation des solveurs avec différentes heuristiques pour améliorer l'efficacité.

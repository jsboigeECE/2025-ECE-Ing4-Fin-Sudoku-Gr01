# Sudoku Backtracking Solver

Ce projet contient des implémentations de solveurs de Sudoku basées sur l'algorithme de **backtracking**. L'objectif est d'illustrer différentes approches de résolution en utilisant C# pur et des intégrations avec Python via **Python.NET**. Ce projet fait partie d'une bibliothèque de classes partagée avec d'autres solveurs, permettant une modularité et une extensibilité pour des projets similaires.

## Solvers Backtracking

### 1. Solveur en C# pur : `BacktrackingDotNetSolver`

Le solveur `BacktrackingDotNetSolver` implémente une approche classique de backtracking directement en C#.

- **Points clés**
  - Récursion pour explorer les solutions possibles.
  - Validation des valeurs avec des vérifications de lignes, colonnes, et blocs 3x3.
  - Affichage du nombre d'appels récursifs effectués pour résoudre la grille.

- **Code source** [`BacktrackingDotNetSolver.cs`](./BacktrackingDotNetSolver.cs)

### 2. Solveur avec Python.NET : `BacktrackingPythonDotNetSolver`

Ce solveur utilise **Python.NET** pour appeler un script Python qui résout un Sudoku en utilisant le solveur `BacktrackingDotNetSolver` écrit en C#.

- **Points clés**
  - Création d'un pont entre C# et Python avec Python.NET.
  - Utilisation d'un script Python pour exécuter le solveur .NET.
  - Exemple d'intégration inter-langages pour étendre les fonctionnalités.

- **Code source** [`BacktrackingPythonDotNetSolver.cs`](./BacktrackingPythonDotNetSolver.cs)

- **Script Python**
  - [`SelfCallSolver.py`](./Resources/SelfCallSolver.py)

### 3. Solveur basé sur NumPy : `BacktrackingPythonSolver`

Ce solveur utilise **NumPy** pour optimiser les calculs dans le script Python.

- **Points clés**
  - Conversion des grilles C# en tableaux NumPy.
  - Validation rapide avec NumPy pour améliorer les performances.
  - Intégration des scripts Python via des ressources embarquées (fichier `.resx`).

- **Code source** [`BacktrackingPythonSolver.cs`](./BacktrackingPythonSolver.cs)

- **Script Python**
  - [`Backtracking.py`](./Resources/Backtracking.py)

## Fichiers embarqués

Les scripts Python nécessaires sont intégrés au projet via un fichier `.resx` :

- [`Resources.resx`](./Resources.resx)
  - Contient les scripts Python pour assurer une portabilité facile.

## Configuration et environnement

### Prérequis

- .NET 9.0 ou supérieur
- Python 3.x
- Bibliothèques Python : `numpy`

### Installation des dépendances Python

Le projet `BacktrackingPythonSolver` gère automatiquement l'installation des dépendances Python comme `numpy` depuis le csharp ce qui permet l'harmonisation de tous les environnemnets de test. Si nécessaire, vous pouvez les installer manuellement :

```bash
pip install numpy
```

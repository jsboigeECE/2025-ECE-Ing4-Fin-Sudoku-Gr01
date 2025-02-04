# Sudoku Solver - Algorithme Génétique et Backtracking

## 🔍 Introduction

Ce projet implémente un solveur de Sudoku hybride combinant :  
- Un algorithme génétique avec la librairie **PyGAD**  
- Un solveur **backtracking** en cas d'échec de PyGAD  
- Un programme en **C#** pour l'exécution et le benchmarking  

L'objectif est de générer une solution valide pour une grille de Sudoku donnée en entrée et d'évaluer les performances des solveurs.

## 💡 Fonctionnalités principales

✔ **Lecture des grilles Sudoku** depuis des fichiers préexistants (ex : `Sudoku_Easy`, `Sudoku_Hardest`).  
✔ **Conversion d'une grille Sudoku** sous forme de texte en une matrice 9x9.  
✔ **Utilisation de PyGAD** pour générer une population initiale et optimiser la solution.  
✔ **Évaluation des grilles** avec une fonction de fitness qui pénalise les erreurs.  
✔ **Sélection, croisement et mutation** pour améliorer les solutions.  
✔ **Correction avec backtracking** si PyGAD ne trouve pas de solution parfaite.  
✔ **Benchmarking des solveurs** avec BenchmarkDotNet en C#.  

## 🗂 Installation

### Prérequis

- **Python 3.6+**  
- **C# .NET Core**  
- Librairies : `numpy`, `pygad`  

### Installation des dépendances

```sh
pip install numpy pygad
```

### Compilation du programme C#

Assurez-vous que **.NET Core** est installé, puis exécutez :

```sh
dotnet build
```

## 🛠️ Utilisation

### 1. Lancer le fichier `Program.cs`

Le programme **Program.cs** doit être lancé directement pour exécuter les solveurs et les benchmarks.  
L'utilisateur devra ensuite choisir un mode :  

- **1** : Tester un solveur sur une grille spécifique  
- **2** : Exécuter un benchmark  
- **3** : Benchmark personnalisé  
- **4** : Quitter  

### Format des fichiers de puzzles

Les grilles Sudoku sont stockées dans des fichiers tels que `Sudoku_Easy` et `Sudoku_Hardest`.  
Le programme les charge directement pour permettre à l'utilisateur de choisir une grille spécifique.

## 🎨 Architecture du Code

### 📌 Fichiers principaux

1. **`Program.cs` (C#)** : Programme principal permettant de tester les solveurs et d'exécuter des benchmarks.  
2. **`PyGadSolver.cs` (C#)** : Interface entre C# et Python, exécute le solveur PyGAD via un script Python.  
3. **`PyGadSolver.py` (Python)** : Algorithme génétique basé sur PyGAD pour résoudre le Sudoku.  

### 🔁 Flux de fonctionnement

1. Lecture et conversion de la grille depuis un fichier de puzzles en matrice 9x9.  
2. Génération d'une **population initiale de 300 Sudokus**.  
3. Évaluation des grilles avec la **fonction de fitness**.  
4. Sélection des meilleures solutions.  
5. Reproduction et mutation des meilleures grilles.  
6. Vérification de la solution.  
7. Utilisation du **backtracking** si PyGAD échoue.  
8. Affichage de la solution finale.  
9. Exécution du **benchmark des solveurs** en C#.  

## 🔧 Exemples de Sortie

✅ **Si une solution est trouvée** :  

```
534678912672195348198342567859761423426853791713924856961537284287419635345286179
```  

❌ **Si aucune solution n'est trouvée** :  

```
000000000000000000000000000000000000000000000000000000000000000000000000000000000
```  

### 📊 Exemple de benchmark

```
Running Benchmark...
Solver A: 150ms
Solver B: 220ms
PyGAD Solver: 180ms
```

## 🚀 Améliorations Possibles

- Ajouter d'autres algorithmes d'optimisation (**Simulated Annealing, Tabu Search**).  
- Améliorer la mutation et la sélection pour accélérer la convergence.  
- Optimiser l'implémentation du **backtracking** pour plus d'efficacité.  
- Intégrer d'autres solveurs dans le **benchmark** pour comparaison.  
- Ajouter des solveurs basés sur des **réseaux neuronaux**.  

## 👤 Auteurs

- **Timothé DO OLIVAL** - Ingénierie et Implémentation  
- **Baptiste ALLAIN** - Ingénierie et Implémentation  
- **Mathis CHATILLON** - Ingénierie et Implémentation  

## 📖 Licence

Ce projet est sous licence libre. Vous pouvez l'utiliser, le modifier et le partager librement.

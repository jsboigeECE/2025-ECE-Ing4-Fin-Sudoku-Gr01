# Solver SMT Z3

Ce projet propose plusieurs implémentations d'un solveur de Sudoku utilisant le moteur SMT [Z3](https://github.com/Z3Prover/z3). L’objectif est d’explorer différentes stratégies et optimisations pour résoudre rapidement et efficacement des grilles de Sudoku. Le projet est écrit en C# et s’appuie sur l’API Microsoft.Z3 pour formuler et résoudre le problème.

---

## Table des matières

- [Introduction](#introduction)
- [Fonctionnalités](#fonctionnalités)
- [Architecture et Extensions](#architecture-et-extensions)
  - [Classe de Base Commune](#classe-de-base-commune)
  - [API de Substitution](#api-de-substitution)
  - [Résolution Incrémentale](#résolution-incrémentale)
    - [Solveur avec Hypothèses](#solveur-avec-hypothèses)
    - [Solveur avec Portées (Push/Pop)](#solveur-avec-portées-pushpop)
  - [Optimisation par Vecteurs de Bits (BitVec)](#optimisation-par-vecteurs-de-bits-bitvec)
  - [Utilisation de Tactiques et Goals](#utilisation-de-tactiques-et-goals)
- [Optimisations et Paramétrages](#optimisations-et-paramétrages)
- [Dépendances et Installation](#dépendances-et-installation)
- [Documentation et Ressources](#documentation-et-ressources)


---

## Introduction

Le solveur SMT Z3 pour Sudoku met en œuvre plusieurs techniques avancées pour améliorer la performance et la flexibilité :
- Utilisation de contraintes SMT pour modéliser les règles du Sudoku.
- Approches incrémentales pour tester différentes hypothèses sans reconstruire l’intégralité du problème.
- Optimisations de la représentation (utilisation de BitVec) pour réduire l’empreinte mémoire.
- Paramétrages fins du moteur Z3 pour ajuster les heuristiques de résolution.

---

## Fonctionnalités

- **Solveur Basique SMT Z3** : Construction des variables pour chaque cellule avec des contraintes sur leur domaine (1 à 9) et l’unicité dans les lignes, colonnes et blocs 3×3.
- **Mutualisation du Code** : Une classe de base (`Z3SolverBase`) regroupe la création des variables et l’ajout des contraintes communes.
- **API de Substitution** : Remplacement direct des variables fixes par leurs constantes dans les contraintes pour simplifier la formule.
- **Résolution Incrémentale** :
  - **Hypothèses** : Utilisation de `solver.Check(assumptions)` pour tester temporairement des contraintes (ex. forcer une cellule à 1).
  - **Portées (Push/Pop)** : Ajout temporaire de contraintes via `Push()` et retrait via `Pop()` pour explorer différentes configurations.
- **Optimisation par Vecteurs de Bits (BitVec)** : Représentation compacte des cellules en utilisant des BitVec de 4 bits, permettant une résolution plus performante.
- **Utilisation de Tactiques et Goals** : Prétraitement du problème en encapsulant les assertions dans un Goal, puis application de tactiques (ex. `"simplify"`) pour réduire la complexité du problème avant la vérification.

---

## Architecture et Extensions

### Classe de Base Commune

La classe `Z3SolverBase` (non montrée ici) centralise la création des variables et des contraintes communes (domaine, lignes, colonnes, blocs). Les solveurs spécifiques héritent de cette classe et redéfinissent la méthode d’ajout des contraintes pour les cellules déjà fixées.

### API de Substitution

En lieu et place d’ajouter une contrainte d’égalité pour chaque cellule fixée, l’API de substitution permet de remplacer directement les variables par leurs constantes dans les contraintes. Cela simplifie la formule et peut améliorer la performance du solveur.

### Résolution Incrémentale

#### Solveur avec Hypothèses

Ce solveur utilise la méthode `Check` avec un ensemble d’hypothèses pour tester des contraintes supplémentaires sans les intégrer définitivement dans le solveur.

#### Solveur avec Portées (Push/Pop)

En utilisant `Push()` et `Pop()`, ce solveur ajoute temporairement des contraintes (par exemple, fixer une cellule non initialisée à une valeur donnée) et revient à l’état de base pour explorer différentes configurations.

### Optimisation par Vecteurs de Bits (BitVec)

La classe `Z3OptimizedSolverBitVec` représente chaque cellule par un BitVec de 4 bits (ce qui suffit pour représenter les valeurs de 1 à 9). Cette approche offre un gain en termes de mémoire et de performance.

### Utilisation de Tactiques et Goals

Le solveur peut encapsuler l’ensemble des assertions dans un `Goal` et appliquer une tactique telle que `"simplify"` pour réduire la complexité du problème avant la vérification. Cela peut accélérer la recherche de solution.

---

## Optimisations et Paramétrages

Vous pouvez jouer avec divers paramètres lors de la création du contexte Z3 pour optimiser la résolution des Sudoku. Par exemple :

```csharp
var cfg = new Dictionary<string, string>
{
    { "auto_config", "false" },          // Désactive l’auto configuration pour un contrôle total.
    { "timeout", "5000" },               // Timeout fixé à 5000 ms.
    { "smt.phase_selection", "5" },      // Influence la stratégie de sélection des phases.
    { "smt.case_split", "4" }            // Ajuste la façon dont le problème est découpé.
    // { "smt.relevancy", "2" }           // Optionnel : ajuste le filtrage par pertinence.
};
```
---
## Documentation et Ressources
Microsoft.Z3 :
Installez le package via NuGet :
````bash
Install-Package Microsoft.Z3.x64 -Version [version]
````
Pour le Partie du code en Python, ne pas oublier d'installer la librairie 
```
pip install z3-solver
```

**.NET Framework / .NET Core :**
- Le projet est compatible avec les environnements .NET.

**Sudoku.Shared :**
- Une bibliothèque partagée contenant la définition de SudokuGrid et l'interface ISudokuSolver.

**Documentation disponible dans le ReadMe général du Projet Sudoku.**
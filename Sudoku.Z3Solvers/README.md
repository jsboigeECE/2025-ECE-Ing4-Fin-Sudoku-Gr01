# Sudoku Z3 Solver

Ce projet contient des implémentations de solveurs de Sudoku utilisant le **SMT solver Z3**. Ces implémentations démontrent comment utiliser des outils avancés de résolution symbolique pour traiter des problèmes algorithmiques complexes comme le Sudoku. Ce projet est une suite logique des solveurs basés sur le backtracking, avec une approche basée sur les contraintes et la logique.

## Solvers Z3

### 1. Solveur en C# : `Z3SimpleSolver`

Le solveur `Z3SimpleSolver` implémente une résolution de Sudoku en utilisant directement la bibliothèque Z3 pour C# (via le package NuGet `Microsoft.Z3`).

- **Points clés**
  - Création d'une matrice 9x9 de variables symboliques représentant les cellules du Sudoku.
  - Ajout de contraintes pour les lignes, colonnes et blocs 3x3, assurant l'unicité des valeurs.
  - Application des valeurs initiales de la grille comme contraintes supplémentaires.
  - Utilisation d'un modèle généré par Z3 pour produire la solution.

- **Code source** [`Z3SimpleSolver.cs`](./Z3SimpleSolver.cs)


### 2. Solveur avec Python.NET : `Z3SimplePythonSolver`

Le solveur `Z3SimplePythonSolver` utilise **Python.NET** pour intégrer un script Python exploitant la bibliothèque Z3 pour Python.

- **Points clés**
  - Conversion directe des grilles C# en structures Python (sans NumPy).
  - Chargement du script Python depuis un fichier (`z3solver.py`) copié dans le répertoire de sortie.
  - Déclaration et manipulation des contraintes dans Python avec Z3, suivie d'une récupération de la solution dans le C#.

- **Code source** [`Z3SimplePythonSolver.cs`](./Z3SimplePythonSolver.cs)

- **Script Python**
  - [`z3solver.py`](./z3solver.py)

## Différences avec l'implémentation Backtracking

- **Approche par contraintes vs Backtracking**
  - Le solver Z3 repose sur une modélisation logique et la recherche d'un modèle satisfaisant (SAT), tandis que le backtracking explore récursivement toutes les solutions possibles.
  - Z3 offre des performances supérieures dans les problèmes bien contraints, mais peut être plus complexe à configurer.

- **Gestion des scripts Python**
  - Contrairement au backtracking qui intègre les scripts Python dans un fichier `.resx`, ici, les scripts sont explicitement copiés dans le répertoire de sortie grâce à la configuration du projet (`csproj`). Cela facilite leur édition et leur portabilité.
  - **Bloc à ajouter au fichier `.csproj`** pour copier le script Python dans le répertoire de sortie (manuellement dans vscode; dans Rider ou VS, utiliser les propriétés du fichier pour le rajouter automatiquement)

```xml
<ItemGroup>
    <None Update="z3solver.py">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```


- **Utilisation de bibliothèques**
  - Z3 en C# est intégré via **NuGet** (package `Microsoft.Z3`).
  - Z3 en Python nécessite **pip** pour installer la bibliothèque `z3-solver`. Cette distinction montre aux étudiants deux systèmes de gestion de dépendances.

## Configuration et environnement

### Prérequis

- .NET 9.0 ou supérieur
- Python 3.x
- Bibliothèques Python : `z3-solver`

### Installation des dépendances Python

Pour exécuter le solveur Python, installez les dépendances nécessaires :

```bash
pip install z3-solver
```

## Extensions possibles pour les solveurs Z3

Pour inciter à l'exploration et à l'amélioration, voici quelques idées d'extensions basées sur des concepts avancés :

- **Héritage pour mutualiser le code** : Implémentez une classe de base commune, comme un `Z3SolverBase`, pour regrouper les contraintes génériques (par exemple, lignes, colonnes, blocs 3x3) et les initialisations. Les solveurs spécifiques peuvent hériter de cette classe et se concentrer sur des stratégies personnalisées, telles que l'utilisation de substitutions ou d'hypothèses.

- **API de substitution** : Utilisez l'API de substitution de Z3 pour appliquer directement les valeurs fixes de la grille d'entrée aux variables symboliques, au lieu d'ajouter manuellement des contraintes pour chaque cellule.

- **Solveurs alternatifs** :
  - **Solveur avec hypothèses** : Expérimentez la méthode `Check` de Z3 en passant des contraintes spécifiques comme hypothèses, permettant de résoudre rapidement des variantes de la grille sans réinitialiser l'état global du solveur.
  - **Utilisation des portées (Scopes)** : Ajoutez des contraintes spécifiques à une grille dans un contexte temporaire à l'aide de `Push` et `Pop`, ce qui permet d'explorer différentes configurations sans réaffecter les contraintes de base.

- **Optimisations via vecteurs de bits** : Représentez les cellules ou les contraintes comme des vecteurs de bits pour réduire l'utilisation de mémoire et maximiser les performances lors de la génération ou de l'évaluation des modèles.

Ces approches offrent des pistes pour diversifier les implémentations tout en utilisant efficacement les capacités avancées de Z3. Les étudiants peuvent ainsi développer des solveurs optimisés et adaptés à des variantes ou extensions du Sudoku.

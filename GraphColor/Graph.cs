using System.Collections.Generic;

namespace GraphColoring
{
    // Définition de la classe Node pour représenter un nœud dans un graphe.
    public class Node
    {
        public int Id { get; private set; }  // Identifiant unique pour chaque nœud.
        public int Data { get; private set; }  // Donnée optionnelle stockée dans le nœud.
        private Dictionary<int, int> ConnectedTo;  // Dictionnaire pour stocker les connexions et leurs poids.

        // Constructeur pour créer un nouveau nœud avec un identifiant et une donnée optionnelle.
        public Node(int idx, int data = 0)
        {
            Id = idx;
            Data = data;
            ConnectedTo = new Dictionary<int, int>();
        }

        // Ajoute une connexion entre ce nœud et un autre nœud avec un poids spécifié.
        public void AddNeighbour(Node neighbour, int weight = 0)
        {
            if (!ConnectedTo.ContainsKey(neighbour.Id))
            {
                ConnectedTo[neighbour.Id] = weight;
            }
        }

        // Met à jour la donnée stockée dans le nœud.
        public void SetData(int data)
        {
            Data = data;
        }

        // Retourne tous les identifiants des nœuds connectés à ce nœud.
        public IEnumerable<int> GetConnections()
        {
            return ConnectedTo.Keys;
        }

        // Récupère le poids de la connexion avec un nœud voisin spécifique.
        public int GetWeight(Node neighbour)
        {
            return ConnectedTo[neighbour.Id];
        }
    }

    // Définition de la classe Graph pour représenter un ensemble de nœuds.
    public class Graph
    {
        public int TotalV { get; private set; }  // Nombre total de nœuds dans le graphe.
        private Dictionary<int, Node> AllNodes;  // Dictionnaire pour stocker tous les nœuds par identifiant.

        // Constructeur pour créer un nouveau graphe vide.
        public Graph()
        {
            TotalV = 0;
            AllNodes = new Dictionary<int, Node>();
        }

        // Ajoute un nœud au graphe avec un identifiant spécifié, retourne le nœud créé ou null si déjà présent.
        public Node? AddNode(int idx)
        {
            if (AllNodes.ContainsKey(idx))
                return null;

            TotalV++;
            var node = new Node(idx: idx);
            AllNodes[idx] = node;
            return node;
        }

        // Ajoute ou met à jour la donnée d'un nœud existant.
        public void AddNodeData(int idx, int data)
        {
            if (AllNodes.ContainsKey(idx))
            {
                AllNodes[idx].SetData(data);
            }
        }

        // Ajoute une arête bidirectionnelle entre deux nœuds spécifiés avec un poids optionnel.
        public void AddEdge(int src, int dst, int wt = 0)
        {
            AllNodes[src].AddNeighbour(AllNodes[dst], wt);
            AllNodes[dst].AddNeighbour(AllNodes[src], wt);
        }

        // Vérifie si deux nœuds sont voisins.
        public bool IsNeighbour(int u, int v)
        {
            if (u >= 1 && u <= 81 && v >= 1 && v <= 81 && u != v)
            {
                return AllNodes[u].GetConnections().Contains(v);
            }
            return false;
        }

        // Récupère un nœud par son identifiant, retourne null si le nœud n'existe pas.
        public Node? GetNode(int idx)
        {
            return AllNodes.ContainsKey(idx) ? AllNodes[idx] : null;
        }

        // Retourne les identifiants de tous les nœuds présents dans le graphe.
        public IEnumerable<int> GetAllNodesIds()
        {
            return AllNodes.Keys;
        }
    }
}

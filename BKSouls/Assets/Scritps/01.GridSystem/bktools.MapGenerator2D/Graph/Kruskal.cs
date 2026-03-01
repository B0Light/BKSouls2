using System.Collections.Generic;
using UnityEngine;

namespace bkTools
{
    public static class Kruskal
    {
        public class Edge : bkTools.Edge
        {
            public float Distance { get; private set; }

            public Edge(Vertex u, Vertex v) : base(u, v)
            {
                Distance = Vector3.Distance(u.Position, v.Position);
            }

            public static bool operator ==(Edge left, Edge right)
            {
                return (left.U == right.U && left.V == right.V) || (left.U == right.V && left.V == right.U);
            }

            public static bool operator !=(Edge left, Edge right)
            {
                return !(left == right);
            }

            public override bool Equals(object obj)
            {
                if (obj is Edge e)
                {
                    return this == e;
                }

                return false;
            }

            public bool Equals(Edge e)
            {
                return this == e;
            }

            public override int GetHashCode()
            {
                return U.GetHashCode() ^ V.GetHashCode();
            }
        }

        public static List<Edge> GetMinimumSpanningTree(List<Edge> edges, List<Vertex> vertices)
        {
            // 1. 간선 정렬
            edges.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            // 2. 유니온-파인드 자료구조 초기화
            var parent = new Dictionary<Vertex, Vertex>();
            var rank = new Dictionary<Vertex, int>();

            foreach (var vertex in vertices)
            {
                parent[vertex] = vertex;
                rank[vertex] = 0;
            }

            Vertex Find(Vertex vertex)
            {
                if (parent[vertex] != vertex)
                {
                    parent[vertex] = Find(parent[vertex]);
                }

                return parent[vertex];
            }

            void Union(Vertex u, Vertex v)
            {
                var rootU = Find(u);
                var rootV = Find(v);

                if (rootU != rootV)
                {
                    if (rank[rootU] > rank[rootV])
                    {
                        parent[rootV] = rootU;
                    }
                    else if (rank[rootU] < rank[rootV])
                    {
                        parent[rootU] = rootV;
                    }
                    else
                    {
                        parent[rootV] = rootU;
                        rank[rootU]++;
                    }
                }
            }

            // 3. MST 구하기
            List<Edge> result = new List<Edge>();

            foreach (var edge in edges)
            {
                var rootU = Find(edge.U);
                var rootV = Find(edge.V);

                if (rootU != rootV)
                {
                    result.Add(edge);
                    Union(edge.U, edge.V);
                }
            }

            return result;
        }
    }
}
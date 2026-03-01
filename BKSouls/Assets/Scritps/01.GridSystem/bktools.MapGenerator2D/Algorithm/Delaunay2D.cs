using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using bkTools;

public class Delaunay2D {
    public class Triangle : IEquatable<Triangle> {
        public Vertex A, B, C;
        public bool IsBad;

        public Triangle(Vertex a, Vertex b, Vertex c) => (A, B, C) = (a, b, c);

        public bool ContainsVertex(Vector3 v) => Vector3.Distance(v, A.Position) < 0.01f
                                               || Vector3.Distance(v, B.Position) < 0.01f
                                               || Vector3.Distance(v, C.Position) < 0.01f;

        public bool CircumCircleContains(Vector3 v) {
            var a = A.Position; var b = B.Position; var c = C.Position;
            float ab = a.sqrMagnitude, cd = b.sqrMagnitude, ef = c.sqrMagnitude;
            float circumX = (ab * (c.y - b.y) + cd * (a.y - c.y) + ef * (b.y - a.y))
                          / (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y));
            float circumY = (ab * (c.x - b.x) + cd * (a.x - c.x) + ef * (b.x - a.x))
                          / (a.y * (c.x - b.x) + b.y * (a.x - c.x) + c.y * (b.x - a.x));
            Vector3 circum = new Vector3(circumX / 2, circumY / 2);
            return Vector3.SqrMagnitude(v - circum) <= Vector3.SqrMagnitude(a - circum);
        }

        public static bool operator ==(Triangle left, Triangle right) =>
            (left.A == right.A || left.A == right.B || left.A == right.C)
         && (left.B == right.A || left.B == right.B || left.B == right.C)
         && (left.C == right.A || left.C == right.B || left.C == right.C);

        public static bool operator !=(Triangle left, Triangle right) => !(left == right);
        public override bool Equals(object obj) => obj is Triangle t && this == t;
        public bool Equals(Triangle t) => this == t;
        public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
    }

    public class Edge : IEquatable<Edge> {
        public Vertex U, V;
        public Edge(Vertex u, Vertex v) => (U, V) = (u, v);

        public static bool operator ==(Edge left, Edge right) => 
            (left.U == right.U || left.U == right.V) && (left.V == right.U || left.V == right.V);

        public static bool operator !=(Edge left, Edge right) => !(left == right);
        public override bool Equals(object obj) => obj is Edge e && this == e;
        public bool Equals(Edge e) => this == e;
        public override int GetHashCode() => U.GetHashCode() ^ V.GetHashCode();

        public static bool AlmostEqual(Edge left, Edge right) => 
            AlmostEqual_V(left.U, right.U) && AlmostEqual_V(left.V, right.V) ||
            AlmostEqual_V(left.U, right.V) && AlmostEqual_V(left.V, right.U);
    }

    static bool AlmostEqual(float x, float y) => 
        Mathf.Abs(x - y) <= float.Epsilon * Mathf.Abs(x + y) * 2 || Mathf.Abs(x - y) < float.MinValue;

    static bool AlmostEqual_V(Vertex left, Vertex right) => 
        AlmostEqual(left.Position.x, right.Position.x) && AlmostEqual(left.Position.y, right.Position.y);

    public List<Vertex> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }
    public List<Triangle> Triangles { get; private set; }

    Delaunay2D() {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
    }

    public static Delaunay2D Triangulate(List<Vertex> vertices) {
        var delaunay = new Delaunay2D { Vertices = new List<Vertex>(vertices) };
        delaunay.Triangulate();
        return delaunay;
    }

    void Triangulate() {
        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        Vertices.ForEach(v => {
            if (v.Position.x < minX) minX = v.Position.x;
            if (v.Position.x > maxX) maxX = v.Position.x;
            if (v.Position.y < minY) minY = v.Position.y;
            if (v.Position.y > maxY) maxY = v.Position.y;
        });

        float deltaMax = Mathf.Max(maxX - minX, maxY - minY) * 2;
        var p1 = new Vertex(new Vector2(minX - 1, minY - 1));
        var p2 = new Vertex(new Vector2(minX - 1, maxY + deltaMax));
        var p3 = new Vertex(new Vector2(maxX + deltaMax, minY - 1));

        Triangles.Add(new Triangle(p1, p2, p3));

        Vertices.ForEach(vertex => {
            var polygon = new List<Edge>();

            // 잘못된 삼각형을 제거하고, 해당 삼각형의 간선을 폴리곤 리스트에 추가
            foreach (var triangle in Triangles.ToList()) {
                if (triangle.CircumCircleContains(vertex.Position)) {
                    polygon.AddRange(new List<Edge> {
                        new Edge(triangle.A, triangle.B),
                        new Edge(triangle.B, triangle.C),
                        new Edge(triangle.C, triangle.A)
                    });
                    triangle.IsBad = true;
                }
            }

            // 잘못된 삼각형 제거
            Triangles.RemoveAll(t => t.IsBad);

            // 중복된 간선을 제거
            polygon = polygon
                .GroupBy(e => e) // Edge를 그룹화하여 중복 탐지
                .Where(g => g.Count() == 1) // 한 번만 등장하는 Edge만 선택
                .Select(g => g.First())
                .ToList();

            // 남은 간선으로 새로운 삼각형 생성
            polygon.ForEach(edge => Triangles.Add(new Triangle(edge.U, edge.V, vertex)));
        });

        Triangles.RemoveAll(t => t.ContainsVertex(p1.Position) || t.ContainsVertex(p2.Position) || t.ContainsVertex(p3.Position));

        var edgeSet = new HashSet<Edge>();
        Triangles.ForEach(t => {
            if (edgeSet.Add(new Edge(t.A, t.B))) Edges.Add(new Edge(t.A, t.B));
            if (edgeSet.Add(new Edge(t.B, t.C))) Edges.Add(new Edge(t.B, t.C));
            if (edgeSet.Add(new Edge(t.C, t.A))) Edges.Add(new Edge(t.C, t.A));
        });
    }
}
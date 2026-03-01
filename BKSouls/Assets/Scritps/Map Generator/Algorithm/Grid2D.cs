using System.Collections.Generic;
using UnityEngine;

public class Grid2D<T> {
    Dictionary<Vector2Int, T> data;

    public Vector2Int Size { get; private set; }
    public Vector2Int Offset { get; set; }

    public Grid2D(Vector2Int size, Vector2Int offset) {
        Size = size;
        Offset = offset;

        data = new Dictionary<Vector2Int, T>();
    }

    public bool InBounds(Vector2Int pos) {
        return new RectInt(Vector2Int.zero, Size).Contains(pos + Offset);
    }

    public T this[int x, int y] {
        get {
            return this[new Vector2Int(x, y)];
        }
        set {
            this[new Vector2Int(x, y)] = value;
        }
    }

    public T this[Vector2Int pos] {
        get {
            pos += Offset;
            if (data.TryGetValue(pos, out var value)) {
                return value;
            } else {
                return default(T); // 기본값 반환, 값이 없을 경우
            }
        }
        set {
            pos += Offset;
            data[pos] = value;
        }
    }

    public IEnumerable<Vector2Int> GetAllPositions() {
        return data.Keys;
    }
}
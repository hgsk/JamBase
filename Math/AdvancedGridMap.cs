using System;
using System.Collections.Generic;
using System.Linq;

public class AdvancedGridMap
{
    public enum CellType
    {
        Empty,
        Occupied,
        Blocked
    }

    private CellType[,] grid;
    private float[,] influenceMap;
    private int[,] heightMap;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public AdvancedGridMap(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new CellType[width, height];
        influenceMap = new float[width, height];
        heightMap = new int[width, height];
    }

    public CellType GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return CellType.Blocked;
        return grid[x, y];
    }

    public void SetCell(int x, int y, CellType type)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            grid[x, y] = type;
    }

    public float GetInfluence(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return 0;
        return influenceMap[x, y];
    }

    public void AddInfluence(int x, int y, float value)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            influenceMap[x, y] += value;
    }

    public int GetHeight(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return 0;
        return heightMap[x, y];
    }

    public void SetHeight(int x, int y, int height)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            heightMap[x, y] = height;
    }

    // 影響マップの更新（拡散）
    public void UpdateInfluenceMap(float decayFactor = 0.9f)
    {
        var newInfluenceMap = new float[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                float totalInfluence = influenceMap[x, y];
                int count = 1;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                        {
                            totalInfluence += influenceMap[nx, ny];
                            count++;
                        }
                    }
                }

                newInfluenceMap[x, y] = (totalInfluence / count) * decayFactor;
            }
        }
        influenceMap = newInfluenceMap;
    }

    // 高さを考慮したA*パスファインディング
    public List<(int, int)> FindPath(int startX, int startY, int endX, int endY, int maxClimbHeight = 1)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<(int, int)>();
        var start = new Node(null, startX, startY);
        var end = new Node(null, endX, endY);

        openSet.Add(start);

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(node => node.FCost).First();
            if (current.X == end.X && current.Y == end.Y)
            {
                return RetracePath(start, current);
            }

            openSet.Remove(current);
            closedSet.Add((current.X, current.Y));

            foreach (var neighbor in GetNeighbors(current, maxClimbHeight))
            {
                if (closedSet.Contains((neighbor.X, neighbor.Y)))
                    continue;

                var newMovementCost = current.GCost + 1 + Math.Abs(GetHeight(neighbor.X, neighbor.Y) - GetHeight(current.X, current.Y));
                if (newMovementCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCost;
                    neighbor.HCost = GetDistance(neighbor, end);
                    neighbor.Parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new List<(int, int)>(); // パスが見つからない場合
    }

    private List<Node> GetNeighbors(Node node, int maxClimbHeight)
    {
        var neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.X + x;
                int checkY = node.Y + y;

                if (checkX >= 0 && checkX < Width && checkY >= 0 && checkY < Height)
                {
                    if (grid[checkX, checkY] != CellType.Blocked &&
                        Math.Abs(GetHeight(checkX, checkY) - GetHeight(node.X, node.Y)) <= maxClimbHeight)
                    {
                        neighbors.Add(new Node(node, checkX, checkY));
                    }
                }
            }
        }
        return neighbors;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Math.Abs(nodeA.X - nodeB.X);
        int dstY = Math.Abs(nodeA.Y - nodeB.Y);
        return dstX + dstY;
    }

    private List<(int, int)> RetracePath(Node startNode, Node endNode)
    {
        var path = new List<(int, int)>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add((currentNode.X, currentNode.Y));
            currentNode = currentNode.Parent;
        }
        path.Reverse();
        return path;
    }

    // 高さと影響力を考慮した視線の確認
    public bool HasLineOfSight(int x1, int y1, int x2, int y2, float influenceThreshold = 0.5f)
    {
        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        int startHeight = GetHeight(x1, y1);

        while (true)
        {
            if (x1 == x2 && y1 == y2) break;
            if (GetCell(x1, y1) == CellType.Blocked) return false;
            if (GetInfluence(x1, y1) > influenceThreshold) return false;
            if (GetHeight(x1, y1) > startHeight + 2) return false; // 高さの差が大きすぎる場合

            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x1 += sx; }
            if (e2 <= dx) { err += dx; y1 += sy; }
        }
        return true;
    }

    // 高さマップの生成（シンプルなダイヤモンドスクエアアルゴリズム）
    public void GenerateHeightMap(int minHeight, int maxHeight, float roughness)
    {
        int size = Math.Max(Width, Height);
        size = (int)Math.Pow(2, Math.Ceiling(Math.Log(size, 2)));

        heightMap = new int[size, size];

        // 四隅の初期化
        heightMap[0, 0] = Random(minHeight, maxHeight);
        heightMap[0, size - 1] = Random(minHeight, maxHeight);
        heightMap[size - 1, 0] = Random(minHeight, maxHeight);
        heightMap[size - 1, size - 1] = Random(minHeight, maxHeight);

        float range = maxHeight - minHeight;
        for (int step = size - 1; step > 1; step /= 2)
        {
            int halfStep = step / 2;

            // ダイヤモンドステップ
            for (int x = halfStep; x < size; x += step)
            {
                for (int y = halfStep; y < size; y += step)
                {
                    DiamondStep(x, y, halfStep, range * roughness);
                }
            }

            // スクエアステップ
            for (int x = 0; x < size; x += halfStep)
            {
                for (int y = (x + halfStep) % step; y < size; y += step)
                {
                    SquareStep(x, y, halfStep, range * roughness);
                }
            }

            range *= roughness;
        }

        // サイズ調整
        if (size != Width || size != Height)
        {
            var newHeightMap = new int[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    newHeightMap[x, y] = heightMap[x, y];
                }
            }
            heightMap = newHeightMap;
        }
    }

    private void DiamondStep(int x, int y, int step, float range)
    {
        int average = (
            heightMap[x - step, y - step] +
            heightMap[x - step, y + step] +
            heightMap[x + step, y - step] +
            heightMap[x + step, y + step]
        ) / 4;

        heightMap[x, y] = (int)(average + RandomRange(-range, range));
    }

    private void SquareStep(int x, int y, int step, float range)
    {
        int count = 0;
        int sum = 0;

        if (x - step >= 0) { sum += heightMap[x - step, y]; count++; }
        if (x + step < heightMap.GetLength(0)) { sum += heightMap[x + step, y]; count++; }
        if (y - step >= 0) { sum += heightMap[x, y - step]; count++; }
        if (y + step < heightMap.GetLength(1)) { sum += heightMap[x, y + step]; count++; }

        heightMap[x, y] = (int)(sum / count + RandomRange(-range, range));
    }

    private int Random(int min, int max)
    {
        return new Random().Next(min, max + 1);
    }

    private float RandomRange(float min, float max)
    {
        return (float)(new Random().NextDouble() * (max - min) + min);
    }

    private class Node
    {
        public Node Parent;
        public int X, Y;
        public int GCost, HCost;
        public int FCost => GCost + HCost;

        public Node(Node parent, int x, int y)
        {
            Parent = parent;
            X = x;
            Y = y;
        }
    }
}

public class AdvancedGridMapExample
{
    public void DemonstrateAdvancedGridMap()
    {
        var map = new AdvancedGridMap(20, 20);

        // 高さマップの生成
        map.GenerateHeightMap(0, 10, 0.5f);

        // いくつかのセルに影響を追加
        map.AddInfluence(5, 5, 1.0f);
        map.AddInfluence(15, 15, 0.8f);

        // 影響マップの更新
        map.UpdateInfluenceMap();

        // パスファインディングのデモ
        var path = map.FindPath(0, 0, 19, 19, 2);
        Console.WriteLine("Path found: " + string.Join(" -> ", path));

        // 視線確認のデモ
        bool hasLineOfSight = map.HasLineOfSight(0, 0, 19, 19);
        Console.WriteLine($"Has line of sight: {hasLineOfSight}");

        // マップの可視化（高さと影響力）
        VisualizeMap(map);
    }

    private void VisualizeMap(AdvancedGridMap map)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                int height = map.GetHeight(x, y);
                float influence = map.GetInfluence(x, y);
                Console.Write($"{height,2:D2}({influence:F1}) ");
            }
            Console.WriteLine();
        }
    }
}

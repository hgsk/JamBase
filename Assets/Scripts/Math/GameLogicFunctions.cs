using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// クラシックおよび現代のゲームで使用される汎用的なゲームロジック関数
/// </summary>
public static class GameLogicFunctions
{
    /// <summary>
    /// 2次元グリッド上の近傍セルを取得する（8方向）
    /// 用途: パックマン、マインスイーパー、ライフゲームなど
    /// </summary>
    public static List<(int, int)> GetNeighbors(int x, int y, int width, int height)
    {
        var neighbors = new List<(int, int)>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    neighbors.Add((nx, ny));
                }
            }
        }
        return neighbors;
    }

    /// <summary>
    /// 線分の交差判定
    /// 用途: 多くの2Dゲーム、特に当たり判定
    /// </summary>
    public static bool LineIntersection(
        float x1, float y1, float x2, float y2,
        float x3, float y3, float x4, float y4)
    {
        float den = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        if (den == 0) return false; // 平行または同一線上

        float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / den;
        float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / den;

        return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
    }

    /// <summary>
    /// タイルベースの視線チェック（Bresenhamのアルゴリズム）
    /// 用途: ローグライク、タクティカルRPGなど
    /// </summary>
    public static bool HasLineOfSight(int x1, int y1, int x2, int y2, Func<int, int, bool> isBlocked)
    {
        int dx = Math.Abs(x2 - x1), sx = x1 < x2 ? 1 : -1;
        int dy = -Math.Abs(y2 - y1), sy = y1 < y2 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            if (x1 == x2 && y1 == y2) break;
            if (isBlocked(x1, y1)) return false;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x1 += sx; }
            if (e2 <= dx) { err += dx; y1 += sy; }
        }
        return true;
    }

    /// <summary>
    /// サイコロの結果を模擬する
    /// 用途: RPG、ボードゲーム、ダンジョン探索ゲームなど
    /// </summary>
    public static int RollDice(int numberOfDice, int sidesPerDie)
    {
        Random rand = new Random();
        int total = 0;
        for (int i = 0; i < numberOfDice; i++)
        {
            total += rand.Next(1, sidesPerDie + 1);
        }
        return total;
    }

    /// <summary>
    /// 簡易的なインベントリ管理システム
    /// 用途: RPG、アドベンチャーゲーム、サバイバルゲームなど
    /// </summary>
    public class Inventory
    {
        private Dictionary<string, int> items = new Dictionary<string, int>();
        private int capacity;

        public Inventory(int capacity)
        {
            this.capacity = capacity;
        }

        public bool AddItem(string item, int quantity = 1)
        {
            if (GetTotalItems() + quantity > capacity) return false;
            if (!items.ContainsKey(item)) items[item] = 0;
            items[item] += quantity;
            return true;
        }

        public bool RemoveItem(string item, int quantity = 1)
        {
            if (!items.ContainsKey(item) || items[item] < quantity) return false;
            items[item] -= quantity;
            if (items[item] == 0) items.Remove(item);
            return true;
        }

        public int GetItemQuantity(string item)
        {
            return items.ContainsKey(item) ? items[item] : 0;
        }

        public int GetTotalItems()
        {
            return items.Values.Sum();
        }
    }

    /// <summary>
    /// 簡易的な状態マシン
    /// 用途: キャラクターAI、ゲームフロー管理など
    /// </summary>
    public class StateMachine<T>
    {
        private Dictionary<T, Action> states = new Dictionary<T, Action>();
        private T currentState;

        public void AddState(T state, Action action)
        {
            states[state] = action;
        }

        public void SetState(T newState)
        {
            if (states.ContainsKey(newState))
            {
                currentState = newState;
            }
        }

        public void Update()
        {
            if (states.ContainsKey(currentState))
            {
                states[currentState]();
            }
        }
    }

    /// <summary>
    /// A*パスファインディングアルゴリズム
    /// 用途: 多くのゲームでのNPC移動、プレイヤーナビゲーションなど
    /// </summary>
    public static List<(int, int)> AStar(
        (int, int) start, (int, int) goal,
        Func<(int, int), List<(int, int)>> getNeighbors,
        Func<(int, int), (int, int), float> heuristic)
    {
        var openSet = new HashSet<(int, int)> { start };
        var cameFrom = new Dictionary<(int, int), (int, int)>();
        var gScore = new Dictionary<(int, int), float> { [start] = 0 };
        var fScore = new Dictionary<(int, int), float> { [start] = heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            var current = openSet.OrderBy(pos => fScore.GetValueOrDefault(pos, float.MaxValue)).First();

            if (current.Equals(goal))
            {
                var path = new List<(int, int)>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            openSet.Remove(current);
            foreach (var neighbor in getNeighbors(current))
            {
                var tentativeGScore = gScore[current] + 1; // Assuming unit cost
                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + heuristic(neighbor, goal);
                    openSet.Add(neighbor);
                }
            }
        }

        return new List<(int, int)>(); // No path found
    }
}

/// <summary>
/// ゲームロジック関数の使用例を示すクラス
/// </summary>
public class GameLogicExample
{
    public void DemonstrateGameLogicFunctions()
    {
        // 近傍セルの取得例（パックマンのゴースト移動など）
        var neighbors = GameLogicFunctions.GetNeighbors(5, 5, 10, 10);
        Console.WriteLine($"Neighbors of (5,5): {string.Join(", ", neighbors)}");

        // 線分の交差判定例（2D格闘ゲームのヒットボックスチェックなど）
        bool intersects = GameLogicFunctions.LineIntersection(0, 0, 10, 10, 0, 10, 10, 0);
        Console.WriteLine($"Lines intersect: {intersects}");

        // 視線チェックの例（タクティカルRPGの攻撃範囲チェックなど）
        bool hasLineOfSight = GameLogicFunctions.HasLineOfSight(0, 0, 5, 5, (x, y) => x == 2 && y == 2);
        Console.WriteLine($"Has line of sight: {hasLineOfSight}");

        // サイコロを振る例（RPGのダメージ計算など）
        int diceRoll = GameLogicFunctions.RollDice(3, 6); // 3d6
        Console.WriteLine($"3d6 roll result: {diceRoll}");

        // インベントリ管理の例
        var inventory = new GameLogicFunctions.Inventory(10);
        inventory.AddItem("Potion", 3);
        inventory.AddItem("Sword", 1);
        Console.WriteLine($"Potions in inventory: {inventory.GetItemQuantity("Potion")}");

        // 状態マシンの例（敵AIなど）
        var enemyAI = new GameLogicFunctions.StateMachine<string>();
        enemyAI.AddState("Patrol", () => Console.WriteLine("Patrolling..."));
        enemyAI.AddState("Chase", () => Console.WriteLine("Chasing player..."));
        enemyAI.SetState("Patrol");
        enemyAI.Update();

        // A*パスファインディングの例
        var path = GameLogicFunctions.AStar(
            (0, 0), (5, 5),
            pos => GameLogicFunctions.GetNeighbors(pos.Item1, pos.Item2, 10, 10),
            (a, b) => Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2)
        );
        Console.WriteLine($"Path found: {string.Join(" -> ", path)}");
    }
}

using System;

public class GridMap
{
    public enum CellType
    {
        Empty,
        Occupied
    }

    private CellType[,] grid;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public GridMap(int width, int height)
    {
        Width = width;
        Height = height;
        grid = new CellType[width, height];
    }

    /// <summary>
    /// エンティティを配置できるかどうかを判定し、可能であれば配置する
    /// </summary>
    /// <param name="entityWidth">エンティティの幅</param>
    /// <param name="entityHeight">エンティティの高さ</param>
    /// <param name="x">配置したい左上のX座標</param>
    /// <param name="y">配置したい左上のY座標</param>
    /// <param name="placeIfPossible">trueの場合、配置可能ならエンティティを配置する</param>
    /// <returns>配置可能であればtrue、そうでなければfalse</returns>
    public bool CanPlaceEntity(int entityWidth, int entityHeight, int x, int y, bool placeIfPossible = false)
    {
        // マップの範囲外をチェック
        if (x < 0 || y < 0 || x + entityWidth > Width || y + entityHeight > Height)
        {
            return false;
        }

        // 配置領域の各セルをチェック
        for (int i = x; i < x + entityWidth; i++)
        {
            for (int j = y; j < y + entityHeight; j++)
            {
                if (grid[i, j] == CellType.Occupied)
                {
                    return false;
                }
            }
        }

        // 配置可能で、placeIfPossibleがtrueの場合、エンティティを配置
        if (placeIfPossible)
        {
            PlaceEntity(entityWidth, entityHeight, x, y);
        }

        return true;
    }

    /// <summary>
    /// エンティティを配置する（内部メソッド）
    /// </summary>
    private void PlaceEntity(int entityWidth, int entityHeight, int x, int y)
    {
        for (int i = x; i < x + entityWidth; i++)
        {
            for (int j = y; j < y + entityHeight; j++)
            {
                grid[i, j] = CellType.Occupied;
            }
        }
    }

    /// <summary>
    /// グリッドの状態を文字列として返す
    /// </summary>
    public string GetGridString()
    {
        var result = new System.Text.StringBuilder();
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                result.Append(grid[i, j] == CellType.Empty ? "." : "X");
            }
            result.AppendLine();
        }
        return result.ToString();
    }
}

public class GridPlacementExample
{
    public void DemonstrateGridPlacement()
    {
        var map = new GridMap(10, 10);

        Console.WriteLine("Initial empty map:");
        Console.WriteLine(map.GetGridString());

        Console.WriteLine("Trying to place a 2x2 entity at (1,1):");
        bool canPlace = map.CanPlaceEntity(2, 2, 1, 1, true);
        Console.WriteLine($"Can place: {canPlace}");
        Console.WriteLine(map.GetGridString());

        Console.WriteLine("Trying to place a 3x3 entity at (7,7):");
        canPlace = map.CanPlaceEntity(3, 3, 7, 7, true);
        Console.WriteLine($"Can place: {canPlace}");
        Console.WriteLine(map.GetGridString());

        Console.WriteLine("Trying to place a 2x2 entity at (0,0):");
        canPlace = map.CanPlaceEntity(2, 2, 0, 0, true);
        Console.WriteLine($"Can place: {canPlace}");
        Console.WriteLine(map.GetGridString());

        Console.WriteLine("Trying to place a 4x4 entity at (3,3):");
        canPlace = map.CanPlaceEntity(4, 4, 3, 3, true);
        Console.WriteLine($"Can place: {canPlace}");
        Console.WriteLine(map.GetGridString());
    }
}

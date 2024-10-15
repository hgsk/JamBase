public class MinesweeperState : IGameState
{
    public bool[,] Mines { get; }
    public bool[,] Revealed { get; }
    public int Size { get; }

    public MinesweeperState(int size, int mineCount)
    {
        Size = size;
        Mines = new bool[size, size];
        Revealed = new bool[size, size];

        // ランダムに地雷を配置
        var random = new System.Random();
        for (int i = 0; i < mineCount; i++)
        {
            int x, y;
            do
            {
                x = random.Next(size);
                y = random.Next(size);
            } while (Mines[x, y]);
            Mines[x, y] = true;
        }
    }

    public void RevealCell(int x, int y)
    {
        Revealed[x, y] = true;
    }

    public bool IsMine(int x, int y) => Mines[x, y];
    public bool IsRevealed(int x, int y) => Revealed[x, y];
}

public class MinesweeperMatchResult : IMatchResult
{
    public bool IsGameOver { get; }
    public bool IsVictory { get; }
    public List<Vector2Int> AffectedCells { get; }

    public MinesweeperMatchResult(bool isGameOver, bool isVictory, List<Vector2Int> affectedCells)
    {
        IsGameOver = isGameOver;
        IsVictory = isVictory;
        AffectedCells = affectedCells;
    }
}

[CreateAssetMenu(fileName = "MinesweeperRevealSpec", menuName = "Specs/Minesweeper/Reveal")]
public class MinesweeperRevealSpec : SpecSO<MinesweeperState, MinesweeperMatchResult>
{
    public Vector2Int RevealedCell;

    public override bool IsSatisfiedBy(MinesweeperState state)
    {
        return !state.IsRevealed(RevealedCell.x, RevealedCell.y);
    }

    public override MinesweeperMatchResult GetMatchResult(MinesweeperState state)
    {
        if (state.IsMine(RevealedCell.x, RevealedCell.y))
        {
            return new MinesweeperMatchResult(true, false, new List<Vector2Int> { RevealedCell });
        }

        var revealedCells = RevealAdjacentCells(state, RevealedCell.x, RevealedCell.y);
        bool isVictory = IsVictoryAchieved(state);
        
        return new MinesweeperMatchResult(isVictory, isVictory, revealedCells);
    }

    private List<Vector2Int> RevealAdjacentCells(MinesweeperState state, int x, int y)
    {
        var revealedCells = new List<Vector2Int>();
        var cellsToCheck = new Queue<Vector2Int>();
        cellsToCheck.Enqueue(new Vector2Int(x, y));

        while (cellsToCheck.Count > 0)
        {
            var cell = cellsToCheck.Dequeue();
            if (state.IsRevealed(cell.x, cell.y)) continue;

            state.RevealCell(cell.x, cell.y);
            revealedCells.Add(cell);

            if (GetAdjacentMineCount(state, cell.x, cell.y) == 0)
            {
                foreach (var adjacent in GetAdjacentCells(state, cell.x, cell.y))
                {
                    cellsToCheck.Enqueue(adjacent);
                }
            }
        }

        return revealedCells;
    }

    private int GetAdjacentMineCount(MinesweeperState state, int x, int y)
    {
        return GetAdjacentCells(state, x, y).Count(cell => state.IsMine(cell.x, cell.y));
    }

    private IEnumerable<Vector2Int> GetAdjacentCells(MinesweeperState state, int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && nx < state.Size && ny >= 0 && ny < state.Size)
                {
                    yield return new Vector2Int(nx, ny);
                }
            }
        }
    }

    private bool IsVictoryAchieved(MinesweeperState state)
    {
        for (int x = 0; x < state.Size; x++)
        {
            for (int y = 0; y < state.Size; y++)
            {
                if (!state.IsMine(x, y) && !state.IsRevealed(x, y))
                {
                    return false;
                }
            }
        }
        return true;
    }
}

// テトリス関連の実装
public enum TetriminoType { I, O, T, S, Z, J, L }

public class TetrisState : IGameState
{
    public int[,] Board { get; }
    public int Width { get; }
    public int Height { get; }

    public TetrisState(int width, int height)
    {
        Width = width;
        Height = height;
        Board = new int[width, height];
    }

    public void SetCell(int x, int y, int value)
    {
        Board[x, y] = value;
    }

    public int GetCell(int x, int y)
    {
        return Board[x, y];
    }
}

public class TetrisMatchResult : IMatchResult
{
    public List<int> ClearedLines { get; }

    public TetrisMatchResult(List<int> clearedLines)
    {
        ClearedLines = clearedLines;
    }
}

[CreateAssetMenu(fileName = "TetrisLineClearSpec", menuName = "Specs/Tetris/LineClear")]
public class TetrisLineClearSpec : SpecSO<TetrisState, TetrisMatchResult>
{
    public override bool IsSatisfiedBy(TetrisState state)
    {
        return GetMatchResult(state).ClearedLines.Any();
    }

    public override TetrisMatchResult GetMatchResult(TetrisState state)
    {
        var clearedLines = new List<int>();

        for (int y = 0; y < state.Height; y++)
        {
            if (IsLineFull(state, y))
            {
                clearedLines.Add(y);
            }
        }

        return new TetrisMatchResult(clearedLines);
    }

    private bool IsLineFull(TetrisState state, int y)
    {
        for (int x = 0; x < state.Width; x++)
        {
            if (state.GetCell(x, y) == 0)
            {
                return false;
            }
        }
        return true;
    }
}

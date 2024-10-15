using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public interface IGameState { }

public interface IMatchResult { }

public interface ISpec<TState, TResult>
    where TState : IGameState
    where TResult : IMatchResult
{
    bool IsSatisfiedBy(TState state);
    TResult GetMatchResult(TState state);
}

public abstract class SpecSO<TState, TResult> : ScriptableObject, ISpec<TState, TResult>
    where TState : IGameState
    where TResult : IMatchResult
{
    public abstract bool IsSatisfiedBy(TState state);
    public abstract TResult GetMatchResult(TState state);
}

[CreateAssetMenu(fileName = "CompositeSpec", menuName = "Specs/Composite")]
public class CompositeSpec<TState, TResult> : SpecSO<TState, TResult>
    where TState : IGameState
    where TResult : IMatchResult
{
    public List<SpecSO<TState, TResult>> specs;
    public bool useAnd = true;

    public override bool IsSatisfiedBy(TState state)
    {
        return useAnd
            ? specs.All(spec => spec.IsSatisfiedBy(state))
            : specs.Any(spec => spec.IsSatisfiedBy(state));
    }

    public override TResult GetMatchResult(TState state)
    {
        // この実装は具体的なTResultの型に依存するため、
        // 実際の使用時に適切にオーバーライドする必要があります。
        throw new System.NotImplementedException();
    }
}

// ゲームボード関連の実装
public class GameBoardState : IGameState
{
    public int[,] Board { get; }
    public int Size { get; }

    public GameBoardState(int size)
    {
        Size = size;
        Board = new int[size, size];
    }

    public int GetCell(int x, int y) => Board[x, y];
    public void SetCell(int x, int y, int value) => Board[x, y] = value;
}

public class GameBoardMatchResult : IMatchResult
{
    public List<Vector2Int> Positions { get; }

    public GameBoardMatchResult(List<Vector2Int> positions)
    {
        Positions = positions;
    }
}

public class Direction
{
    public int RowDelta { get; }
    public int ColDelta { get; }

    public Direction(int rowDelta, int colDelta)
    {
        RowDelta = rowDelta;
        ColDelta = colDelta;
    }

    public static readonly Direction Horizontal = new Direction(0, 1);
    public static readonly Direction Vertical = new Direction(1, 0);
    public static readonly Direction DiagonalDown = new Direction(1, 1);
    public static readonly Direction DiagonalUp = new Direction(-1, 1);
}

[CreateAssetMenu(fileName = "ThreeInARowSameColorSpec", menuName = "Specs/ThreeInARowSameColor")]
public class ThreeInARowSameColorSpec : SpecSO<GameBoardState, GameBoardMatchResult>
{
    public Direction direction;

    public override bool IsSatisfiedBy(GameBoardState state)
    {
        return GetMatchResult(state).Positions.Any();
    }

    public override GameBoardMatchResult GetMatchResult(GameBoardState state)
    {
        int size = state.Size;
        int rowStart = direction.RowDelta < 0 ? 2 : 0;
        int rowEnd = size - (direction.RowDelta > 0 ? 2 : 0);
        int colEnd = size - (direction.ColDelta > 0 ? 2 : 0);

        for (int row = rowStart; row < rowEnd; row++)
        {
            for (int col = 0; col < colEnd; col++)
            {
                var match = CheckThreeInARow(state, row, col);
                if (match.Count == 3)
                {
                    return new GameBoardMatchResult(match);
                }
            }
        }

        return new GameBoardMatchResult(new List<Vector2Int>());
    }

    private List<Vector2Int> CheckThreeInARow(GameBoardState state, int startRow, int startCol)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int value = state.GetCell(startRow, startCol);
        if (value == 0) return positions;

        for (int i = 0; i < 3; i++)
        {
            int row = startRow + i * direction.RowDelta;
            int col = startCol + i * direction.ColDelta;
            if (state.GetCell(row, col) != value)
                return new List<Vector2Int>();
            positions.Add(new Vector2Int(row, col));
        }
        return positions;
    }
}

[CreateAssetMenu(fileName = "ThreeMatchWinCondition", menuName = "WinConditions/ThreeMatch")]
public class ThreeMatchWinCondition : SpecSO<GameBoardState, GameBoardMatchResult>
{
    public List<ThreeInARowSameColorSpec> directionSpecs;

    private void OnEnable()
    {
        if (directionSpecs == null || directionSpecs.Count == 0)
        {
            directionSpecs = new List<ThreeInARowSameColorSpec>
            {
                CreateThreeInARowSpec(Direction.Horizontal),
                CreateThreeInARowSpec(Direction.Vertical),
                CreateThreeInARowSpec(Direction.DiagonalDown),
                CreateThreeInARowSpec(Direction.DiagonalUp)
            };
        }
    }

    private ThreeInARowSameColorSpec CreateThreeInARowSpec(Direction direction)
    {
        var spec = CreateInstance<ThreeInARowSameColorSpec>();
        spec.direction = direction;
        return spec;
    }

    public override bool IsSatisfiedBy(GameBoardState state)
    {
        return directionSpecs.Any(spec => spec.IsSatisfiedBy(state));
    }

    public override GameBoardMatchResult GetMatchResult(GameBoardState state)
    {
        foreach (var spec in directionSpecs)
        {
            var result = spec.GetMatchResult(state);
            if (result.Positions.Any())
            {
                return result;
            }
        }
        return new GameBoardMatchResult(new List<Vector2Int>());
    }
}

// ポーカー関連の実装例
public enum CardSuit { Hearts, Diamonds, Clubs, Spades }
public enum CardRank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

public class Card
{
    public CardSuit Suit { get; }
    public CardRank Rank { get; }

    public Card(CardSuit suit, CardRank rank)
    {
        Suit = suit;
        Rank = rank;
    }
}

public class PokerHandState : IGameState
{
    public List<Card> Hand { get; }

    public PokerHandState(List<Card> hand)
    {
        Hand = hand;
    }
}

public class PokerHandMatchResult : IMatchResult
{
    public string HandName { get; }
    public List<Card> MatchingCards { get; }

    public PokerHandMatchResult(string handName, List<Card> matchingCards)
    {
        HandName = handName;
        MatchingCards = matchingCards;
    }
}

[CreateAssetMenu(fileName = "PairSpec", menuName = "Specs/Poker/Pair")]
public class PairSpec : SpecSO<PokerHandState, PokerHandMatchResult>
{
    public override bool IsSatisfiedBy(PokerHandState state)
    {
        return state.Hand.GroupBy(card => card.Rank).Any(group => group.Count() == 2);
    }

    public override PokerHandMatchResult GetMatchResult(PokerHandState state)
    {
        var pair = state.Hand.GroupBy(card => card.Rank)
            .FirstOrDefault(group => group.Count() == 2);

        if (pair != null)
        {
            return new PokerHandMatchResult("Pair", pair.ToList());
        }

        return new PokerHandMatchResult("No Pair", new List<Card>());
    }
}

public class GameManager : MonoBehaviour
{
    public ThreeMatchWinCondition threeMatchCondition;
    public PairSpec pairSpec;

    private GameBoardState gameBoardState;
    private PokerHandState pokerHandState;

    void Start()
    {
        gameBoardState = new GameBoardState(6);
        pokerHandState = new PokerHandState(new List<Card>
        {
            new Card(CardSuit.Hearts, CardRank.Ace),
            new Card(CardSuit.Spades, CardRank.Ace),
            new Card(CardSuit.Diamonds, CardRank.King),
            new Card(CardSuit.Clubs, CardRank.Queen),
            new Card(CardSuit.Hearts, CardRank.Jack)
        });
    }

    public void CheckGameConditions()
    {
        // 3マッチゲームの条件チェック
        if (threeMatchCondition.IsSatisfiedBy(gameBoardState))
        {
            var result = threeMatchCondition.GetMatchResult(gameBoardState);
            Debug.Log("Three Match Found!");
            foreach (Vector2Int pos in result.Positions)
            {
                Debug.Log($"Match at position: ({pos.x}, {pos.y})");
            }
        }

        // ポーカーの役（ペア）のチェック
        if (pairSpec.IsSatisfiedBy(pokerHandState))
        {
            var result = pairSpec.GetMatchResult(pokerHandState);
            Debug.Log($"Poker Hand: {result.HandName}");
            foreach (var card in result.MatchingCards)
            {
                Debug.Log($"Matching Card: {card.Rank} of {card.Suit}");
            }
        }
    }
}

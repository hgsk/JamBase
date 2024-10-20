// IScoreData.cs
public interface IScoreData<T>
{
    T Score { get; set; }
    event System.Action<T> OnScoreChanged;
}

// ScoreData.cs
public class ScoreData<T> : IScoreData<T>
{
    private T _score;
    public T Score
    {
        get => _score;
        set
        {
            if (!Equals(_score, value))
            {
                _score = value;
                OnScoreChanged?.Invoke(_score);
            }
        }
    }

    public event System.Action<T> OnScoreChanged;
}

// IScoreView.cs
public interface IScoreView<T>
{
    void UpdateScoreDisplay(T score);
}

// ScorePresenter.cs
public class ScorePresenter<T>
{
    private readonly IScoreData<T> _scoreData;
    private readonly IScoreView<T> _scoreView;

    public ScorePresenter(IScoreData<T> scoreData, IScoreView<T> scoreView)
    {
        _scoreData = scoreData;
        _scoreView = scoreView;
        _scoreData.OnScoreChanged += UpdateScore;
    }

    private void UpdateScore(T newScore)
    {
        _scoreView.UpdateScoreDisplay(newScore);
    }

    public void SetScore(T score)
    {
        _scoreData.Score = score;
    }

    public void Dispose()
    {
        _scoreData.OnScoreChanged -= UpdateScore;
    }
}

// IScoreOperations.cs
public interface IScoreOperations<T>
{
    T Add(T a, T b);
    bool IsGreaterThan(T a, T b);
}

// IntScoreOperations.cs
public class IntScoreOperations : IScoreOperations<int>
{
    public int Add(int a, int b) => a + b;
    public bool IsGreaterThan(int a, int b) => a > b;
}

// Unity Adapter classes:

// UnityScoreData.cs
using UnityEngine;

public class UnityScoreData<T> : ScriptableObject, IScoreData<T>
{
    [SerializeField] private T _score;
    public T Score
    {
        get => _score;
        set
        {
            if (!Equals(_score, value))
            {
                _score = value;
                OnScoreChanged?.Invoke(_score);
            }
        }
    }

    public event System.Action<T> OnScoreChanged;
}

// UnityScoreView.cs
using UnityEngine;
using TMPro;

public class UnityScoreView<T> : MonoBehaviour, IScoreView<T>
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void UpdateScoreDisplay(T score)
    {
        _scoreText.text = $"Score: {score}";
    }
}

// UnityScorePresenter.cs
using UnityEngine;

public class UnityScorePresenter<T> : MonoBehaviour
{
    [SerializeField] private UnityScoreData<T> _scoreData;
    [SerializeField] private UnityScoreView<T> _scoreView;

    private ScorePresenter<T> _presenter;
    private IScoreOperations<T> _scoreOperations;

    private void Start()
    {
        _presenter = new ScorePresenter<T>(_scoreData, _scoreView);
        _scoreOperations = GetScoreOperations();
    }

    private void OnDestroy()
    {
        _presenter.Dispose();
    }

    public void AddScore(T amount)
    {
        T newScore = _scoreOperations.Add(_scoreData.Score, amount);
        _presenter.SetScore(newScore);
    }

    private IScoreOperations<T> GetScoreOperations()
    {
        if (typeof(T) == typeof(int))
        {
            return new IntScoreOperations() as IScoreOperations<T>;
        }
        // Add other type-specific operations here
        throw new System.NotSupportedException($"Score operations for type {typeof(T)} are not supported.");
    }
}

// Usage example:
// UnityIntScorePresenter.cs
public class UnityIntScorePresenter : UnityScorePresenter<int> { }

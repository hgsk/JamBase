using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public interface IMetricStrategy
{
    string Name { get; }
    float Calculate();
    float Weight { get; }
}

[System.Serializable]
public class MetricConfig
{
    public string name;
    public float weight = 1f;
}

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[CreateAssetMenu(fileName = "New Int Performance Metric", menuName = "Game Performance/Int Metric")]
public class IntMetricSO : ScriptableObject, IMetricStrategy
{
    public string Name => config.name;
    public float Weight => config.weight;

    public MetricConfig config;
    public IntEvent calculateEvent;

    public float Calculate()
    {
        if (calculateEvent == null || calculateEvent.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning($"Calculate method not set for metric: {Name}");
            return 0;
        }
        int result = 0;
        calculateEvent.Invoke(result);
        return result;
    }
}

[CreateAssetMenu(fileName = "New Float Performance Metric", menuName = "Game Performance/Float Metric")]
public class FloatMetricSO : ScriptableObject, IMetricStrategy
{
    public string Name => config.name;
    public float Weight => config.weight;

    public MetricConfig config;
    public FloatEvent calculateEvent;

    public float Calculate()
    {
        if (calculateEvent == null || calculateEvent.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning($"Calculate method not set for metric: {Name}");
            return 0f;
        }
        float result = 0f;
        calculateEvent.Invoke(result);
        return result;
    }
}

public interface IScoreCalculationStrategy
{
    float CalculateScore(List<IMetricStrategy> metrics);
}

[CreateAssetMenu(fileName = "New Score Calculator", menuName = "Game Performance/Score Calculator")]
public class ScoreCalculatorSO : ScriptableObject, IScoreCalculationStrategy
{
    public virtual float CalculateScore(List<IMetricStrategy> metrics)
    {
        float totalScore = 0f;
        float totalWeight = 0f;

        foreach (var metric in metrics)
        {
            totalScore += metric.Calculate() * metric.Weight;
            totalWeight += metric.Weight;
        }

        return totalWeight > 0 ? totalScore / totalWeight : 0f;
    }
}

public class PerformanceEvaluator : MonoBehaviour
{
    public List<ScriptableObject> metricConfigs = new List<ScriptableObject>();
    private List<IMetricStrategy> activeMetrics = new List<IMetricStrategy>();

    public ScoreCalculatorSO scoreCalculator;

    private void Start()
    {
        InitializeMetrics();
    }

    private void InitializeMetrics()
    {
        activeMetrics.Clear();
        foreach (var metricConfig in metricConfigs)
        {
            if (metricConfig is IMetricStrategy)
            {
                var metricInstance = Instantiate(metricConfig) as IMetricStrategy;
                activeMetrics.Add(metricInstance);
            }
            else
            {
                Debug.LogWarning($"Invalid metric type: {metricConfig.name}");
            }
        }
    }

    public float CalculateOverallScore()
    {
        if (scoreCalculator == null)
        {
            Debug.LogWarning("Score calculator not set. Using default calculation.");
            return DefaultCalculateScore();
        }
        return scoreCalculator.CalculateScore(activeMetrics);
    }

    private float DefaultCalculateScore()
    {
        float totalScore = 0f;
        float totalWeight = 0f;

        foreach (var metric in activeMetrics)
        {
            totalScore += metric.Calculate() * metric.Weight;
            totalWeight += metric.Weight;
        }

        return totalWeight > 0 ? totalScore / totalWeight : 0f;
    }

    public string GenerateReport()
    {
        string report = "Performance Report:\n";
        foreach (var metric in activeMetrics)
        {
            report += $"{metric.Name}: {metric.Calculate()} (Weight: {metric.Weight})\n";
        }
        report += $"Overall Score: {CalculateOverallScore()}";
        return report;
    }
}

// Example usage in a GameManager
public partial class GameManager : MonoBehaviour
{
    public PerformanceEvaluator evaluator;

    public int playerScore = 100;
    public float playerAccuracy = 0.75f;
    public int remainingTime = 60;
    public float playerHealth = 75.5f;

    void Start()
    {
        // Optionally set a custom score calculator
        // evaluator.scoreCalculator = ScriptableObject.CreateInstance<CustomScoreCalculatorSO>();
    }

    void Update()
    {
        // Game logic...
    }

    void OnGameEnd()
    {
        Debug.Log(evaluator.GenerateReport());
    }

    // These methods will be called by UnityEvents
    public void GetPlayerScore(int result)
    {
        result = playerScore;
    }

    public void GetPlayerAccuracy(float result)
    {
        result = playerAccuracy;
    }

    public void GetRemainingTime(int result)
    {
        result = remainingTime;
    }

    public void GetPlayerHealth(float result)
    {
        result = playerHealth;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 評価関数のためのデリゲート
/// </summary>
/// <param name="state">評価対象の状態</param>
/// <returns>評価値</returns>
public delegate float EvaluationFunction(object state);

/// <summary>
/// ゲーム開発で使用される評価関数のコレクション
/// </summary>
public static class EvaluationFunctions
{
    /// <summary>
    /// 複数の評価関数を組み合わせて新しい評価関数を作成する
    /// </summary>
    /// <param name="evaluations">評価関数のリスト</param>
    /// <param name="weights">各評価関数の重み</param>
    /// <returns>組み合わされた評価関数</returns>
    public static EvaluationFunction Compose(List<EvaluationFunction> evaluations, List<float> weights)
    {
        if (evaluations.Count != weights.Count)
            throw new ArgumentException("Evaluations and weights must have the same length");

        return state =>
        {
            float totalScore = 0;
            float totalWeight = 0;

            for (int i = 0; i < evaluations.Count; i++)
            {
                totalScore += evaluations[i](state) * weights[i];
                totalWeight += weights[i];
            }

            return totalWeight > 0 ? totalScore / totalWeight : 0;
        };
    }

    /// <summary>
    /// 単純なスコア計算
    /// </summary>
    public static EvaluationFunction SimpleScore = state =>
    {
        var (baseScore, multiplier, bonus) = ((int, float, int))state;
        return baseScore * multiplier + bonus;
    };

    /// <summary>
    /// コンボベースのスコア計算
    /// </summary>
    public static EvaluationFunction ComboScore = state =>
    {
        var (baseScore, comboCount, comboMultiplier) = ((int, int, float))state;
        return baseScore * (1 + (comboCount - 1) * comboMultiplier);
    };

    /// <summary>
    /// 時間ベースのスコア計算（速いほど高スコア）
    /// </summary>
    public static EvaluationFunction TimeBasedScore = state =>
    {
        var (baseScore, timeSpent, parTime) = ((int, float, float))state;
        float timeRatio = Math.Min(parTime / timeSpent, 2); // 最大2倍まで
        return baseScore * timeRatio;
    };

    /// <summary>
    /// チェスの駒の価値評価（簡易版）
    /// </summary>
    public static EvaluationFunction ChessMaterialValue = state =>
    {
        var (pawns, knights, bishops, rooks, queens) = ((int, int, int, int, int))state;
        return pawns * 1 + knights * 3 + bishops * 3 + rooks * 5 + queens * 9;
    };

    /// <summary>
    /// RPGのキャラクター強さ評価
    /// </summary>
    public static EvaluationFunction RPGCharacterStrength = state =>
    {
        var (level, attack, defense, speed, hp) = ((int, float, float, float, float))state;
        return (level * 0.5f + attack * 0.3f + defense * 0.2f + speed * 0.1f + hp * 0.2f) * (1 + level * 0.01f);
    };

    /// <summary>
    /// 戦略ゲームの領土価値評価
    /// </summary>
    public static EvaluationFunction TerritoryValue = state =>
    {
        var (resourceValue, strategicImportance, threatLevel) = ((float, float, float))state;
        return resourceValue * (1 + strategicImportance) / (1 + threatLevel);
    };

    /// <summary>
    /// AIの行動選択のための評価関数
    /// </summary>
    public static EvaluationFunction AIActionValue = state =>
    {
        var (immediateGain, futurePotential, risk) = ((float, float, float))state;
        return (immediateGain + futurePotential * 0.5f) * (1 - risk);
    };

    /// <summary>
    /// プレイヤーのスキル評価
    /// </summary>
    public static EvaluationFunction PlayerSkill = state =>
    {
        var (winRate, averageScore, gamesPlayed) = ((float, float, int))state;
        float experienceFactor = Math.Min(gamesPlayed / 100f, 1); // 最大1
        return (winRate * 1000 + averageScore / 100) * (0.5f + experienceFactor * 0.5f);
    };

    /// <summary>
    /// ゲームの難易度評価
    /// </summary>
    public static EvaluationFunction GameDifficulty = state =>
    {
        var (playerSkill, gameComplexity, timeLimit) = ((float, float, float))state;
        float timeFactor = timeLimit > 0 ? 100 / timeLimit : 0;
        return (gameComplexity / playerSkill) * (1 + timeFactor);
    };
}

/// <summary>
/// 評価関数の使用例を示すクラス
/// </summary>
public class EvaluationFunctionExample
{
    public void DemonstrateComposableEvaluationFunctions()
    {
        // RPGキャラクターの総合評価関数を作成
        var rpgCharacterEvaluation = EvaluationFunctions.Compose(
            new List<EvaluationFunction>
            {
                EvaluationFunctions.RPGCharacterStrength,
                state => 
                {
                    var (_, _, _, speed, _) = ((int, float, float, float, float))state;
                    return speed * 2; // 速度に特に重点を置く
                },
                state =>
                {
                    var (level, _, _, _, _) = ((int, float, float, float, float))state;
                    return level * 10; // レベルにボーナスを与える
                }
            },
            new List<float> { 1f, 0.5f, 0.3f }
        );

        // RPGキャラクターの評価
        var characterState = (10, 50f, 30f, 40f, 100f); // (level, attack, defense, speed, hp)
        float characterValue = rpgCharacterEvaluation(characterState);
        Console.WriteLine($"RPG Character Value: {characterValue}");

        // 戦略ゲームの領土評価関数を作成
        var territoryEvaluation = EvaluationFunctions.Compose(
            new List<EvaluationFunction>
            {
                EvaluationFunctions.TerritoryValue,
                state =>
                {
                    var (resourceValue, _, _) = ((float, float, float))state;
                    return resourceValue * 0.5f; // 資源値に追加のウェイト
                },
                state =>
                {
                    var (_, strategicImportance, _) = ((float, float, float))state;
                    return strategicImportance * 100; // 戦略的重要性にボーナス
                }
            },
            new List<float> { 1f, 0.5f, 0.5f }
        );

        // 領土の評価
        var territoryState = (1000f, 0.8f, 0.3f); // (resourceValue, strategicImportance, threatLevel)
        float territoryValue = territoryEvaluation(territoryState);
        Console.WriteLine($"Territory Value: {territoryValue}");

        // ゲームの総合スコア計算関数を作成
        var gameScoreEvaluation = EvaluationFunctions.Compose(
            new List<EvaluationFunction>
            {
                EvaluationFunctions.SimpleScore,
                EvaluationFunctions.ComboScore,
                EvaluationFunctions.TimeBasedScore
            },
            new List<float> { 0.5f, 0.3f, 0.2f }
        );

        // ゲームスコアの評価
        var scoreState = (1000, 1.5f, 500); // SimpleScore state
        var comboState = (500, 5, 0.2f); // ComboScore state
        var timeState = (2000, 60f, 90f); // TimeBasedScore state
        float gameScore = gameScoreEvaluation(scoreState) * 0.5f +
                          gameScoreEvaluation(comboState) * 0.3f +
                          gameScoreEvaluation(timeState) * 0.2f;
        Console.WriteLine($"Game Score: {gameScore}");
    }
}

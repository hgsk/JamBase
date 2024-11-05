using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ゲーム開発で使用される抽選処理関数のコレクション
/// </summary>
public static class LotteryFunctions
{
    private static Random random = new Random();

    /// <summary>
    /// リストからランダムに1つの要素を選択する
    /// </summary>
    public static T ChooseRandom<T>(List<T> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("List is empty or null");
        
        int index = random.Next(items.Count);
        return items[index];
    }

    /// <summary>
    /// 指定された確率で成功するかどうかを判定する
    /// </summary>
    /// <param name="successProbability">成功確率（0.0から1.0）</param>
    public static bool TryProbability(double successProbability)
    {
        return random.NextDouble() < successProbability;
    }

    /// <summary>
    /// 重み付きの抽選を行う
    /// </summary>
    /// <param name="weights">各要素の重み</param>
    /// <returns>選択されたインデックス</returns>
    public static int WeightedRandomSelection(List<double> weights)
    {
        if (weights == null || weights.Count == 0)
            throw new ArgumentException("Weights list is empty or null");

        double totalWeight = weights.Sum();
        double randomValue = random.NextDouble() * totalWeight;
        
        for (int i = 0; i < weights.Count; i++)
        {
            if (randomValue < weights[i])
                return i;
            randomValue -= weights[i];
        }

        return weights.Count - 1; // 丸め誤差対策
    }

    /// <summary>
    /// フィッシャー–イェーツのシャッフルアルゴリズムを使用してリストをシャッフルする
    /// </summary>
    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// ガチャシステムの実装（複数のレアリティを持つアイテムの抽選）
    /// </summary>
    /// <param name="rarityProbabilities">各レアリティの確率（合計が1になるようにする）</param>
    /// <returns>選択されたレアリティのインデックス</returns>
    public static int GachaSystem(List<double> rarityProbabilities)
    {
        return WeightedRandomSelection(rarityProbabilities);
    }

    /// <summary>
    /// 指定された範囲内でユニークな整数のセットを生成する
    /// </summary>
    /// <param name="count">生成する数</param>
    /// <param name="minValue">最小値（含む）</param>
    /// <param name="maxValue">最大値（含む）</param>
    public static HashSet<int> GenerateUniqueRandomNumbers(int count, int minValue, int maxValue)
    {
        if (count > (maxValue - minValue + 1))
            throw new ArgumentException("Count is larger than the range of possible values");

        HashSet<int> numbers = new HashSet<int>();
        while (numbers.Count < count)
        {
            numbers.Add(random.Next(minValue, maxValue + 1));
        }
        return numbers;
    }

    /// <summary>
    /// ポアソン分布に従う乱数を生成する
    /// </summary>
    /// <param name="lambda">ポアソン分布の平均値</param>
    public static int PoissonRandom(double lambda)
    {
        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;

        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > L);

        return k - 1;
    }

    /// <summary>
    /// 正規分布（ガウス分布）に従う乱数を生成する（ボックス=ミュラー法）
    /// </summary>
    /// <param name="mean">平均値</param>
    /// <param name="standardDeviation">標準偏差</param>
    public static double NormalRandom(double mean, double standardDeviation)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + standardDeviation * randStdNormal;
    }

    /// <summary>
    /// レベルアップ時の経験値要求量を計算する（指数関数的増加）
    /// </summary>
    /// <param name="baseXP">基準経験値</param>
    /// <param name="growthRate">成長率</param>
    /// <param name="level">現在のレベル</param>
    public static int CalculateRequiredXP(int baseXP, double growthRate, int level)
    {
        return (int)(baseXP * Math.Pow(growthRate, level - 1));
    }
}

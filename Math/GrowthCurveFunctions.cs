using System;

/// <summary>
/// ゲーム開発で使用される成長曲線関数のコレクション
/// </summary>
public static class GrowthCurveFunctions
{
    /// <summary>
    /// 線形成長曲線
    /// </summary>
    /// <param name="x">現在の値（通常はレベルや時間）</param>
    /// <param name="m">傾き</param>
    /// <param name="b">切片</param>
    public static double Linear(double x, double m, double b)
    {
        return m * x + b;
    }

    /// <summary>
    /// 指数関数的成長曲線
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="a">初期値</param>
    /// <param name="b">成長率</param>
    public static double Exponential(double x, double a, double b)
    {
        return a * Math.Pow(b, x);
    }

    /// <summary>
    /// 対数関数的成長曲線
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="a">スケーリング係数</param>
    /// <param name="b">底</param>
    public static double Logarithmic(double x, double a, double b)
    {
        return a * Math.Log(x, b);
    }

    /// <summary>
    /// シグモイド関数（S字カーブ）
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="L">曲線の最大値</param>
    /// <param name="k">曲線の急峻さ</param>
    /// <param name="x0">変曲点のx座標</param>
    public static double Sigmoid(double x, double L, double k, double x0)
    {
        return L / (1 + Math.Exp(-k * (x - x0)));
    }

    /// <summary>
    /// べき関数的成長曲線
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="a">係数</param>
    /// <param name="b">指数</param>
    public static double Power(double x, double a, double b)
    {
        return a * Math.Pow(x, b);
    }

    /// <summary>
    /// 二次関数的成長曲線
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="a">2次の係数</param>
    /// <param name="b">1次の係数</param>
    /// <param name="c">定数項</param>
    public static double Quadratic(double x, double a, double b, double c)
    {
        return a * x * x + b * x + c;
    }

    /// <summary>
    /// ステップ関数（階段状の成長）
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="stepSize">ステップのサイズ</param>
    /// <param name="stepHeight">各ステップでの増加量</param>
    public static double Step(double x, double stepSize, double stepHeight)
    {
        return Math.Floor(x / stepSize) * stepHeight;
    }

    /// <summary>
    /// サイン波による周期的成長
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="amplitude">振幅</param>
    /// <param name="frequency">周波数</param>
    /// <param name="phase">位相</param>
    public static double SineWave(double x, double amplitude, double frequency, double phase)
    {
        return amplitude * Math.Sin(frequency * x + phase);
    }

    /// <summary>
    /// 複合成長曲線（線形 + 指数）
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="a">線形成分の係数</param>
    /// <param name="b">指数成分の基数</param>
    /// <param name="c">指数成分の係数</param>
    public static double CompoundLinearExponential(double x, double a, double b, double c)
    {
        return a * x + c * Math.Pow(b, x);
    }

    /// <summary>
    /// 対数的な漸近線を持つ成長曲線
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="max">最大値（漸近線）</param>
    /// <param name="steepness">曲線の急峻さ</param>
    public static double LogisticGrowth(double x, double max, double steepness)
    {
        return max / (1 + Math.Exp(-steepness * x));
    }

    /// <summary>
    /// ダイミニッシング・リターンズ曲線（逓減収益曲線）
    /// </summary>
    /// <param name="x">現在の値</param>
    /// <param name="max">最大値</param>
    /// <param name="rate">初期の成長率</param>
    public static double DiminishingReturns(double x, double max, double rate)
    {
        return max * (1 - Math.Exp(-rate * x));
    }

    /// <summary>
    /// カスタム成長曲線（例：RPGのレベルアップに必要な経験値）
    /// </summary>
    /// <param name="level">現在のレベル</param>
    /// <param name="baseXP">基準経験値</param>
    /// <param name="growthFactor">成長係数</param>
    public static int CustomRPGLevelUpXP(int level, int baseXP, double growthFactor)
    {
        return (int)(baseXP * Math.Pow(level, growthFactor));
    }
}

/// <summary>
/// 成長曲線関数の使用例を示すクラス
/// </summary>
public class GrowthCurveExample
{
    public void DemonstrateGrowthCurves()
    {
        // キャラクターの体力成長（線形）
        for (int level = 1; level <= 10; level++)
        {
            double hp = GrowthCurveFunctions.Linear(level, 10, 100);
            Console.WriteLine($"Level {level} HP: {hp}");
        }

        // 武器の攻撃力成長（指数関数的）
        for (int upgrade = 0; upgrade <= 5; upgrade++)
        {
            double attack = GrowthCurveFunctions.Exponential(upgrade, 10, 1.5);
            Console.WriteLine($"Upgrade {upgrade} Attack: {attack}");
        }

        // ゲーム難易度の調整（シグモイド）
        for (int stage = 1; stage <= 20; stage++)
        {
            double difficulty = GrowthCurveFunctions.Sigmoid(stage, 100, 0.5, 10);
            Console.WriteLine($"Stage {stage} Difficulty: {difficulty}");
        }

        // RPGのレベルアップに必要な経験値（カスタム関数）
        for (int level = 1; level <= 10; level++)
        {
            int requiredXP = GrowthCurveFunctions.CustomRPGLevelUpXP(level, 100, 1.5);
            Console.WriteLine($"XP required for Level {level}: {requiredXP}");
        }
    }
}

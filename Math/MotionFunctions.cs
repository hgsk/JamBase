using UnityEngine;
using System;

/// <summary>
/// 基本的な数学関数を提供する名前空間
/// </summary>
namespace MathFunctions
{
    /// <summary>
    /// 基本的な数学演算を行う静的クラス
    /// </summary>
    public static class Basic
    {
        /// <summary>
        /// 線形補間を行う
        /// </summary>
        /// <param name="a">開始値</param>
        /// <param name="b">終了値</param>
        /// <param name="t">補間パラメータ（0から1の間）</param>
        /// <returns>補間された値</returns>
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>
        /// 逆線形補間を行う
        /// </summary>
        /// <param name="a">範囲の開始値</param>
        /// <param name="b">範囲の終了値</param>
        /// <param name="value">補間する値</param>
        /// <returns>0から1の間の補間パラメータ</returns>
        public static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);

        /// <summary>
        /// ある範囲の値を別の範囲に再マッピングする
        /// </summary>
        public static float Remap(float iMin, float iMax, float oMin, float oMax, float value) =>
            Lerp(oMin, oMax, InverseLerp(iMin, iMax, value));
    }

    /// <summary>
    /// 三角関数を提供する静的クラス
    /// </summary>
    public static class Trigonometry
    {
        /// <summary>
        /// 度数法でのサイン関数
        /// </summary>
        public static float SinDeg(float degrees) => Mathf.Sin(degrees * Mathf.Deg2Rad);

        /// <summary>
        /// 度数法でのコサイン関数
        /// </summary>
        public static float CosDeg(float degrees) => Mathf.Cos(degrees * Mathf.Deg2Rad);

        /// <summary>
        /// 度数法でのタンジェント関数
        /// </summary>
        public static float TanDeg(float degrees) => Mathf.Tan(degrees * Mathf.Deg2Rad);
    }

    /// <summary>
    /// イージング関数を提供する静的クラス
    /// </summary>
    public static class Easing
    {
        /// <summary>
        /// 二次関数による緩入りイージング
        /// </summary>
        public static float EaseInQuad(float t) => t * t;

        /// <summary>
        /// 二次関数による緩出しイージング
        /// </summary>
        public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);

        /// <summary>
        /// 二次関数による緩入り緩出しイージング
        /// </summary>
        public static float EaseInOutQuad(float t) =>
            t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

        // 他のイージング関数...
    }
}

/// <summary>
/// 様々な運動関数を提供する名前空間
/// </summary>
namespace MotionFunctions
{
    /// <summary>
    /// 2次元の運動関数を提供する名前空間
    /// </summary>
    namespace TwoDimensional
    {
        /// <summary>
        /// 円運動関連の関数を提供する静的クラス
        /// </summary>
        public static class Circular
        {
            /// <summary>
            /// 2D円運動の位置を計算する
            /// </summary>
            /// <param name="radius">円の半径</param>
            /// <param name="angularSpeed">角速度（ラジアン/秒）</param>
            /// <param name="time">経過時間</param>
            /// <returns>円運動上の2D位置</returns>
            public static Vector2 CircularMotion(float radius, float angularSpeed, float time)
            {
                float x = radius * Mathf.Cos(angularSpeed * time);
                float y = radius * Mathf.Sin(angularSpeed * time);
                return new Vector2(x, y);
            }

            /// <summary>
            /// 2D楕円運動の位置を計算する
            /// </summary>
            /// <param name="a">楕円の長軸</param>
            /// <param name="b">楕円の短軸</param>
            /// <param name="angularSpeed">角速度（ラジアン/秒）</param>
            /// <param name="time">経過時間</param>
            /// <returns>楕円運動上の2D位置</returns>
            public static Vector2 EllipticalMotion(float a, float b, float angularSpeed, float time)
            {
                float x = a * Mathf.Cos(angularSpeed * time);
                float y = b * Mathf.Sin(angularSpeed * time);
                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// 振動運動関連の関数を提供する静的クラス
        /// </summary>
        public static class Oscillation
        {
            /// <summary>
            /// サイン波の値を計算する
            /// </summary>
            public static float SineWave(float amplitude, float frequency, float time) =>
                amplitude * Mathf.Sin(frequency * time);

            /// <summary>
            /// 矩形波の値を計算する
            /// </summary>
            public static float SquareWave(float amplitude, float frequency, float time) =>
                amplitude * Mathf.Sign(Mathf.Sin(frequency * time));
        }
    }

    /// <summary>
    /// 3次元の運動関数を提供する名前空間
    /// </summary>
    namespace ThreeDimensional
    {
        /// <summary>
        /// スパイラル（螺旋）運動関連の関数を提供する静的クラス
        /// </summary>
        public static class Spiral
        {
            /// <summary>
            /// 3Dスパイラル運動の位置を計算する
            /// </summary>
            public static Vector3 SpiralMotion(float radius, float angularSpeed, float verticalSpeed, float time)
            {
                float x = radius * Mathf.Cos(angularSpeed * time);
                float y = radius * Mathf.Sin(angularSpeed * time);
                float z = verticalSpeed * time;
                return new Vector3(x, y, z);
            }

            /// <summary>
            /// 3Dヘリカル（螺旋）運動の位置を計算する
            /// </summary>
            public static Vector3 HelicalMotion(float radius, float pitch, float angularSpeed, float time)
            {
                float angle = angularSpeed * time;
                float x = radius * Mathf.Cos(angle);
                float y = radius * Mathf.Sin(angle);
                float z = pitch * angle / (2 * Mathf.PI);
                return new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// 複雑な3D運動関数を提供する静的クラス
        /// </summary>
        public static class Complex
        {
            /// <summary>
            /// 3Dリサージュ曲線の位置を計算する
            /// </summary>
            public static Vector3 LissajousMotion(float a, float b, float c, float alpha, float beta, float gamma, float time)
            {
                float x = a * Mathf.Sin(alpha * time);
                float y = b * Mathf.Sin(beta * time);
                float z = c * Mathf.Sin(gamma * time);
                return new Vector3(x, y, z);
            }

            /// <summary>
            /// スクリュー運動の位置と回転を計算する
            /// </summary>
            public static (Vector3 position, Quaternion rotation) ScrewMotion(Vector3 axis, float angularSpeed, float linearSpeed, float time)
            {
                Quaternion rotation = Quaternion.AngleAxis(angularSpeed * time * Mathf.Rad2Deg, axis);
                Vector3 translation = axis.normalized * linearSpeed * time;
                return (translation, rotation);
            }
        }
    }

    /// <summary>
    /// パラメトリック曲線関連の関数を提供する名前空間
    /// </summary>
    namespace Parametric
    {
        /// <summary>
        /// 様々なパラメトリック曲線の関数を提供する静的クラス
        /// </summary>
        public static class Curves
        {
            /// <summary>
            /// 2次ベジェ曲線上の点を計算する
            /// </summary>
            public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            {
                float u = 1 - t;
                return u * u * p0 + 2 * u * t * p1 + t * t * p2;
            }

            /// <summary>
            /// Catmull-Rom スプライン曲線上の点を計算する
            /// </summary>
            public static Vector3 CatmullRomSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;

                Vector3 a = -p0 + 3 * p1 - 3 * p2 + p3;
                Vector3 b = 2 * p0 - 5 * p1 + 4 * p2 - p3;
                Vector3 c = -p0 + p2;
                Vector3 d = 2 * p1;

                return 0.5f * (a * t3 + b * t2 + c * t + d);
            }
        }
    }
}

/// <summary>
/// 物理シミュレーション関連の関数を提供する名前空間
/// </summary>
namespace PhysicsSimulation
{
    /// <summary>
    /// 運動学的計算を行う静的クラス
    /// </summary>
    public static class Kinematics
    {
        /// <summary>
        /// 等加速度運動での変位を計算する
        /// </summary>
        public static float CalculateDisplacement(float initialVelocity, float acceleration, float time) =>
            initialVelocity * time + 0.5f * acceleration * time * time;

        /// <summary>
        /// 等加速度運動での終速度を計算する
        /// </summary>
        public static float CalculateFinalVelocity(float initialVelocity, float acceleration, float time) =>
            initialVelocity + acceleration * time;
    }

    /// <summary>
    /// 動力学的計算を行う静的クラス
    /// </summary>
    public static class Dynamics
    {
        /// <summary>
        /// 2つの物体間の重力を計算する
        /// </summary>
        public static Vector3 CalculateGravitationalForce(float mass1, float mass2, Vector3 position1, Vector3 position2)
        {
            const float G = 6.67430e-11f; // 重力定数
            Vector3 direction = position2 - position1;
            float distance = direction.magnitude;
            return G * mass1 * mass2 / (distance * distance) * direction.normalized;
        }
    }
}

/// <summary>
/// 運動関数の使用例を示すクラス
/// </summary>
public class MotionExample : MonoBehaviour
{
    public float radius = 5f;
    public float angularSpeed = 2f;
    public float time = 0f;

    void Update()
    {
        time += Time.deltaTime;

        // 2D円運動の例
        Vector2 circularPosition = MotionFunctions.TwoDimensional.Circular.CircularMotion(radius, angularSpeed, time);
        transform.position = new Vector3(circularPosition.x, circularPosition.y, 0);

        // 3Dスパイラル運動の例
        Vector3 spiralPosition = MotionFunctions.ThreeDimensional.Spiral.SpiralMotion(radius, angularSpeed, 1f, time);
        if (transform.childCount > 0)
            transform.GetChild(0).localPosition = spiralPosition;

        // イージング関数の使用例
        float easedValue = MathFunctions.Easing.EaseInOutQuad(Mathf.PingPong(time, 1));
        transform.localScale = Vector3.one * (1 + easedValue);
    }
}

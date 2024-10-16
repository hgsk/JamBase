using System;
using System.Collections.Generic;

public static class BiomeGenerator
{
    public enum BiomeType
    {
        Ocean,
        Beach,
        Desert,
        Savanna,
        TropicalRainforest,
        Grassland,
        Forest,
        Taiga,
        Tundra,
        SnowyCaps,
        Cave
    }

    private static readonly SimplexNoise Noise = new SimplexNoise();

    public static BiomeType[,,] Generate3DBiome(int width, int height, int depth, float scale)
    {
        var biome = new BiomeType[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    float elevation = GetElevation(x, y, z, scale);
                    float temperature = GetTemperature(x, y, z, scale);
                    float humidity = GetHumidity(x, y, z, scale);

                    biome[x, y, z] = DetermineBiome(elevation, temperature, humidity);
                }
            }
        }

        return biome;
    }

    private static float GetElevation(int x, int y, int z, float scale)
    {
        float elevationBase = (float)Noise.Evaluate(x * scale, y * scale, z * scale);
        float elevationDetail = (float)Noise.Evaluate(x * scale * 4, y * scale * 4, z * scale * 4) * 0.5f;
        return (elevationBase + elevationDetail) / 1.5f;
    }

    private static float GetTemperature(int x, int y, int z, float scale)
    {
        float temperatureBase = (float)Noise.Evaluate(x * scale + 1000, y * scale + 1000, z * scale + 1000);
        float latitudeEffect = 1 - Math.Abs(y - 0.5f) * 2; // Equator is warmer
        return (temperatureBase + latitudeEffect) / 2;
    }

    private static float GetHumidity(int x, int y, int z, float scale)
    {
        return (float)Noise.Evaluate(x * scale + 2000, y * scale + 2000, z * scale + 2000);
    }

    private static BiomeType DetermineBiome(float elevation, float temperature, float humidity)
    {
        if (elevation < -0.5f)
            return BiomeType.Ocean;
        if (elevation < -0.4f)
            return BiomeType.Beach;
        if (elevation > 0.8f)
            return BiomeType.SnowyCaps;
        if (elevation < -0.2f && temperature < 0.2f)
            return BiomeType.Tundra;
        if (elevation < 0f && temperature < 0.3f)
            return BiomeType.Taiga;
        if (temperature > 0.7f && humidity < 0.3f)
            return BiomeType.Desert;
        if (temperature > 0.6f && humidity < 0.5f)
            return BiomeType.Savanna;
        if (temperature > 0.6f && humidity > 0.6f)
            return BiomeType.TropicalRainforest;
        if (humidity < 0.4f)
            return BiomeType.Grassland;
        if (elevation < 0.2f)
            return BiomeType.Cave;

        return BiomeType.Forest;
    }

    public static string Visualize3DBiome(BiomeType[,,] biome, int layer)
    {
        int width = biome.GetLength(0);
        int depth = biome.GetLength(2);

        var visualization = new System.Text.StringBuilder();

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                visualization.Append(GetBiomeChar(biome[x, layer, z]));
            }
            visualization.AppendLine();
        }

        return visualization.ToString();
    }

    private static char GetBiomeChar(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Ocean: return '~';
            case BiomeType.Beach: return '.';
            case BiomeType.Desert: return 'd';
            case BiomeType.Savanna: return 's';
            case BiomeType.TropicalRainforest: return 'r';
            case BiomeType.Grassland: return ',';
            case BiomeType.Forest: return 'f';
            case BiomeType.Taiga: return 't';
            case BiomeType.Tundra: return '_';
            case BiomeType.SnowyCaps: return '^';
            case BiomeType.Cave: return 'o';
            default: return '?';
        }
    }
}

// シンプレックスノイズの実装（簡略化版）
public class SimplexNoise
{
    public double Evaluate(double x, double y, double z)
    {
        // 簡略化のため、実際のシンプレックスノイズの代わりにシンプルな周期関数を使用
        return (Math.Sin(x) + Math.Sin(y) + Math.Sin(z)) / 3.0;
    }
}

public class BiomeGeneratorExample
{
    public void Demonstrate3DBiomeGeneration()
    {
        int width = 50;
        int height = 20;
        int depth = 50;
        float scale = 0.1f;

        var biome = BiomeGenerator.Generate3DBiome(width, height, depth, scale);

        Console.WriteLine("3D Biome Generation Example");
        Console.WriteLine("Visualization of a horizontal slice (y = 10):");
        Console.WriteLine(BiomeGenerator.Visualize3DBiome(biome, 10));
    }
}

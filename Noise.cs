using UnityEngine;

public class Noise
{
    /**
     * Generate Map
     */
    public static float[,] GenerateMap(float mapWidth, float mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[(int)mapWidth, (int)mapHeight];

        if (scale <= 0)
            scale = 0.0001f;

        // Generate random seed
        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        // Set vector offsets based on random seed
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) + offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Min and max noise height values
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Make noiseScale zoom to center, so calculate half of the map
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Loop through each x and y coordinate
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {

                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;

                if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Loop through them again and lerp the three height values together, smoothing the map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
